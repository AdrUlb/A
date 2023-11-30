using ALang.Parsing;
using ALang.Parsing.Declarations;
using ALang.Parsing.Expressions;
using ALang.Parsing.Statements;
using System.Collections.Frozen;
using System.Numerics;

namespace ALang.Ir;

internal enum IrType
{
	// Temporary ambigious types that may not exist after the final type inference pass
	Unknown,
	Number,

	Bool,
	Nothing,
	U16
}

internal abstract class IrInstruction()
{

}

internal sealed class AddInstruction(int leftSourceIndex, int rightSourceReg, int destReg) : IrInstruction
{
	public readonly int LeftSourceIndex = leftSourceIndex;
	public readonly int RightSourceIndex = rightSourceReg;
	public readonly int DestReg = destReg;
}

internal sealed class CompareEqualInstruction(int leftSourceReg, int rightSourceReg, int destReg) : IrInstruction
{
	public readonly int LeftSourceIndex = leftSourceReg;
	public readonly int RightSourceIndex = rightSourceReg;
	public readonly int DestReg = destReg;
}

internal sealed class GetArgumentInstruction(int argumentIndex, int destReg) : IrInstruction
{
	public readonly int ArgumentIndex = argumentIndex;
	public readonly int DestReg = destReg;
}

internal sealed class StoreConstInstruction(BigInteger value, int destReg) : IrInstruction
{
	public readonly BigInteger Value = value;
	public readonly int DestReg = destReg;
}

internal sealed class JumpIfTrueInstruction(int sourceReg) : IrInstruction
{
	public int JumpOffset = 0;
	public readonly int SourceReg = sourceReg;
}

internal sealed class ReturnInstruction(int sourceReg) : IrInstruction
{
	public readonly int SourceReg = sourceReg;
}

internal readonly struct IrFunctionParameter(string name, IrType type)
{
	public readonly string Name = name;
	public readonly IrType Type = type;
}

internal readonly struct IrVariable(string name, IrType type)
{
	public readonly string Name = name;
	public readonly IrType Type = type;
}

internal sealed class IrScope(IrScope? parent)
{
	public readonly IrScope? Parent = parent;

	public readonly Dictionary<string, int> variables = [];

	public void DeclareVariable(string name, int registerIndex)
	{
		variables.Add(name, registerIndex);
	}

	public int GetVariableRegister(string name)
	{
		if (variables.TryGetValue(name, out var index))
			return index;

		if (Parent == null)
			throw new("Variable not declared.");

		return Parent.GetVariableRegister(name);
	}
}

internal sealed class IrFunction(string name, IrType returnType, IrFunctionParameter[] parameters)
{
	public readonly string Name = name;
	public IrType ReturnType = returnType;
	public readonly IrFunctionParameter[] Parameters = parameters;
	public readonly List<IrInstruction> Instructions = [];
	public readonly List<IrType> Registers = [];

	private int FindParameterIndex(string name)
	{
		for (var i = 0; i < Parameters.Length; i++)
		{
			if (Parameters[i].Name == name)
				return i;
		}

		return -1;
	}
}

internal sealed class IrGenerator(IEnumerable<Statement> statements)
{
	private static readonly FrozenDictionary<string, IrType> _typeNames = new Dictionary<string, IrType>
	{
		{ "u16", IrType.U16 }
	}.ToFrozenDictionary();

	private readonly List<IrFunction> _functions = [];
	private readonly Stack<IrScope> _scopeStack = new();

	private IrScope? CurrentScope => _scopeStack.Count != 0 ? _scopeStack.Peek() : null;

	private IrFunction? _currentFunction = null;

	private static IrType GetIrTypeByName(string? name)
	{
		if (name == null)
			return IrType.Nothing;

		if (!_typeNames.TryGetValue(name, out var returnType))
			throw new NotImplementedException();

		return returnType;
	}

	private void EnterScope() => _scopeStack.Push(new(CurrentScope));

	private void LeaveScope() => _scopeStack.Pop();

	public void Generate()
	{
		foreach (var statement in statements)
		{
			switch (statement)
			{
				case FunctionDeclaration s:
					{
						var parameters = new IrFunctionParameter[s.Parameters.Count];

						for (var i = 0; i < parameters.Length; i++)
							parameters[i] = new(s.Parameters[i].Name, GetIrTypeByName(s.Parameters[i].TypeName));

						_currentFunction = new(s.Name, GetIrTypeByName(s.ReturnType), parameters);
						_functions.Add(_currentFunction);

						EnterScope();

						for (int i = 0; i < s.Parameters.Count; i++)
						{
							var param = s.Parameters[i];
							var type = GetIrTypeByName(param.TypeName);
							var registerIndex = _currentFunction.Registers.Count;
							_currentFunction.Registers.Add(type);
							CurrentScope!.DeclareVariable(param.Name, registerIndex);
							_currentFunction.Instructions.Add(new GetArgumentInstruction(i, registerIndex));
						}

						GenerateStatement(s.Body);

						LeaveScope();

						_currentFunction = null;
						break;
					}
				default:
					throw new NotImplementedException(Util.StringifyStatement(statement));
			}
		}
	}

	private void GenerateStatement(Statement statement)
	{
		switch (statement)
		{
			case BlockStatement s:
				{
					EnterScope();

					foreach (var stmt in s.Statements)
						GenerateStatement(stmt);

					LeaveScope();
					break;
				}
			case IfStatement s:
				var conditionIndex = GenerateExpression(s.Condition);
				_currentFunction!.Instructions.Add(new JumpIfTrueInstruction(conditionIndex));
				break;
			case ReturnStatement s:
				{
					int regIndex;
					if (s.Value == null)
					{
						regIndex = _currentFunction!.Registers.Count;
						_currentFunction.Registers.Add(IrType.Nothing);
					}
					else
					{
						regIndex = GenerateExpression(s.Value);
						_currentFunction!.Registers.Add(IrType.Unknown);
					}

					_currentFunction.Instructions.Add(new ReturnInstruction(regIndex));
					break;
				}
			default:
				throw new NotImplementedException(Util.StringifyStatement(statement));
		}
	}

	private int GenerateExpression(Expression expression)
	{
		switch (expression)
		{
			case BinaryExpression e:
				{
					var leftResultIndex = GenerateExpression(e.Left);
					var rightResultIndex = GenerateExpression(e.Right);

					var regIndex = _currentFunction!.Registers.Count;

					switch (e.Operation)
					{
						case BinaryOperation.Add:
							_currentFunction.Instructions.Add(new AddInstruction(leftResultIndex, rightResultIndex, regIndex));
							_currentFunction.Registers.Add(IrType.Unknown);
							break;
						case BinaryOperation.CompareEqual:
							_currentFunction.Instructions.Add(new CompareEqualInstruction(leftResultIndex, rightResultIndex, regIndex));
							_currentFunction.Registers.Add(IrType.Bool);
							break;
						default:
							throw new NotImplementedException(e.Operation.ToString());
					}

					return regIndex;
				}
			case NumericLiteralExpression e:
				{
					if (!BigInteger.TryParse(e.SourceFileFragment.Text, out var value))
						throw new($"Failed to parse numeric literal '{e.SourceFileFragment.Text}'");

					var regIndex = _currentFunction!.Registers.Count;

					_currentFunction.Instructions.Add(new StoreConstInstruction(value, regIndex));
					_currentFunction.Registers.Add(IrType.Unknown);

					return regIndex;
				}
			case ReferenceExpression e:
				var registerIndex = CurrentScope!.GetVariableRegister(e.Name);
				return registerIndex;
			default:
				throw new NotImplementedException(Util.StringifyExpression(expression));
		}
	}
}
