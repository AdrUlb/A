using ALang.Parsing;
using ALang.Parsing.Declarations;
using ALang.Parsing.Expressions;
using ALang.Parsing.Statements;
using System.Collections.Frozen;
using System.Numerics;

namespace ALang.Ir;

internal interface IrType
{

}

internal struct UnknownIrType : IrType
{
	public override readonly string ToString() => "unknown";
}
internal struct IntegerIrType : IrType
{
	public bool Signed = true;
	public int Bits = -1;

	public IntegerIrType()
	{

	}

	public IntegerIrType(bool signed, int bits)
	{
		Signed = signed;
		Bits = bits;
	}

	public override readonly string ToString() => $"{(Signed ? 'i' : 'u')}{(Bits >= 0 ? Bits.ToString() : "?")}";
}
internal struct NothingIrType : IrType
{
	public override readonly string ToString() => "nothing";
}
internal struct BoolIrType : IrType
{
	public override readonly string ToString() => "bool";
}

internal abstract class IrInstruction()
{

}

internal abstract class BinaryOperationInstruction(int leftSourceReg, int rightSourceReg, int destReg) : IrInstruction
{
	public readonly int LeftSourceReg = leftSourceReg;
	public readonly int RightSourceReg = rightSourceReg;
	public readonly int DestReg = destReg;
}

internal sealed class AddInstruction(int leftSourceReg, int rightSourceReg, int destReg)
	: BinaryOperationInstruction(leftSourceReg, rightSourceReg, destReg)
{ }

internal sealed class SubtractInstruction(int leftSourceReg, int rightSourceReg, int destReg)
	: BinaryOperationInstruction(leftSourceReg, rightSourceReg, destReg)
{ }

internal sealed class CompareEqualInstruction(int leftSourceReg, int rightSourceReg, int destReg)
	: BinaryOperationInstruction(leftSourceReg, rightSourceReg, destReg)
{ }

internal sealed class StoreArgumentInstruction(int argumentIndex, int destReg) : IrInstruction
{
	public readonly int ArgumentIndex = argumentIndex;
	public readonly int DestReg = destReg;
}

internal sealed class StoreConstInstruction(BigInteger value, int destReg) : IrInstruction
{
	public readonly BigInteger Value = value;
	public readonly int DestReg = destReg;
}

internal sealed class CondBranchInstruction(int sourceReg) : IrInstruction
{
	public int TrueBranch = -1;
	public int FalseBranch = -1;
	public readonly int SourceReg = sourceReg;
}

internal sealed class PhiInstruction(int branch1, int reg1, int branch2, int reg2, int destReg) : IrInstruction
{
	public int Branch1 = branch1;
	public int Reg1 = reg1;
	public int Branch2 = branch2;
	public int Reg2 = reg2;
	public int DestReg = destReg;
}

internal sealed class BranchInstruction : IrInstruction
{
	public int Branch = -1;
}

internal sealed class ReturnInstruction(int sourceReg) : IrInstruction
{
	public readonly int SourceReg = sourceReg;
}

internal sealed class CallFunctionInstruction(int funcIndex, int[] argRegIndices, int destReg) : IrInstruction
{
	public readonly int FuncIndex = funcIndex;
	public readonly int[] ArgRegIndices = argRegIndices;
	public readonly int DestReg = destReg;
}

internal sealed class CallFunctionByNameInstruction(string name, int[] argRegIndices, int destReg) : IrInstruction
{
	public readonly string Name = name;
	public readonly int[] ArgRegIndices = argRegIndices;
	public readonly int DestReg = destReg;
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

internal sealed class IrBranch(IrBranch? parent)
{
	public readonly IrBranch? Parent = parent;

	public readonly Dictionary<string, int> Map = [];
	public readonly List<(string Name, int NewReg)> Vars = [];

	public void DeclareVariable(string name, int newReg)
	{
		Map.Add(name, newReg);
		Vars.Add((name, newReg));
	}

	public void Reassign(string name, int newReg)
	{
		Map[name] = newReg;
		for (var i = 0; i < Vars.Count; i++)
		{
			if (Vars[i].Name == name)
			{
				Vars[i] = (name, newReg);
				return;
			}
		}

		throw new();
	}

	public bool TryGetVariable(string name, out int newReg)
	{
		if (Map.TryGetValue(name, out newReg))
			return true;

		if (Parent == null)
			return false;

		return Parent.TryGetVariable(name, out newReg);
	}
}

internal sealed class IrScope(IrScope? parent)
{
	public readonly IrScope? Parent = parent;

	public readonly Dictionary<string, int> Variables = [];

	private IrBranch? branch = null;

	public void BeginBranch()
	{
		branch = new(branch);
	}

	public IrBranch EndBranch()
	{
		var b = branch!;
		branch = b.Parent;
		return b;
	}

	public void DeclareVariable(string name, int register)
	{
		Variables.Add(name, register);
	}

	public void ReassignVariable(string name, int register)
	{
		if (branch != null)
		{
			if (!branch.Map.ContainsKey(name))
				branch.DeclareVariable(name, register);
			else
				branch.Reassign(name, register);
			return;
		}

		if (Variables.ContainsKey(name))
		{
			Variables[name] = register;
			return;
		}

		if (Parent == null)
			throw new("Variable not declared.");

		Parent.ReassignVariable(name, register);
	}

	public int GetVariable(string name)
	{
		int ret;

		if (Variables.TryGetValue(name, out var index))
		{
			ret = index;
		}
		else
		{
			if (Parent == null)
				throw new("Variable not declared.");

			ret = Parent.GetVariable(name);
		}

		if (branch != null && branch.TryGetVariable(name, out var value))
			return value;

		return ret;
	}
}

internal sealed class IrFunction(string name, IrType returnType, IrFunctionParameter[] parameters)
{
	public readonly string Name = name;
	public IrType ReturnType = returnType;
	public readonly IrFunctionParameter[] Parameters = parameters;
	public readonly List<IrType> Registers = [];
	public readonly List<IrInstruction> Instructions = [];
	public readonly List<int> Branches = [];

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

internal sealed class IrGenerator
{
	private const int defaultIntSize = 16;

	private static readonly FrozenDictionary<string, IrType> _typeNames = new Dictionary<string, IrType>
	{
		{ "u16", new IntegerIrType(false, 16) }
	}.ToFrozenDictionary();

	internal readonly List<IrFunction> _functions = [];
	private readonly Stack<IrScope> _scopeStack = new();

	private IrScope? CurrentScope => _scopeStack.Count != 0 ? _scopeStack.Peek() : null;

	private IrFunction? _currentFunction = null;

	private static IrType GetIrTypeByName(string? name)
	{
		if (name == null)
			return new NothingIrType();

		if (!_typeNames.TryGetValue(name, out var returnType))
			throw new NotImplementedException();

		return returnType;
	}

	private int GetFunctionIndexByName(string name)
	{
		if (name == "print")
			return -1;

		for (var i = 0; i < _functions.Count; i++)
		{
			if (_functions[i].Name == name)
				return i;
		}

		throw new($"Function '{name}' not found.");
	}

	IrFunction printFunc = new("print", new NothingIrType(), [new IrFunctionParameter("num", new IntegerIrType(false, 16))]);

	private IrFunction GetFunctionByIndex(int index)
	{
		return index switch
		{
			-1 => printFunc,
			_ => _functions[index],
		};
	}

	private void EnterScope() => _scopeStack.Push(new(CurrentScope));

	private void LeaveScope() => _scopeStack.Pop();

	public void Generate(IEnumerable<Statement> statements)
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
							_currentFunction.Instructions.Add(new StoreArgumentInstruction(i, registerIndex));
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

		ResolveFunctionReferences();

		while (InferTypes())
			;
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
			case ExpressionStatement s:
				GenerateExpression(s.Expression);
				break;
			case IfStatement s:
				var conditionIndex = GenerateExpression(s.Condition);
				var branchInstr = new CondBranchInstruction(conditionIndex);
				var branchEndTrueInstr = new BranchInstruction();
				var branchEndFalseInstr = new BranchInstruction();
				_currentFunction!.Instructions.Add(branchInstr);

				branchInstr.TrueBranch = _currentFunction.Branches.Count;
				_currentFunction.Branches.Add(_currentFunction.Instructions.Count);
				CurrentScope!.BeginBranch();
				GenerateStatement(s.Body);
				_currentFunction!.Instructions.Add(branchEndTrueInstr);
				var trueBranch = CurrentScope.EndBranch();

				branchInstr.FalseBranch = _currentFunction.Branches.Count;
				_currentFunction.Branches.Add(_currentFunction.Instructions.Count);
				CurrentScope.BeginBranch();
				if (s.ElseBody != null)
				{
					GenerateStatement(s.ElseBody);
				}
				_currentFunction!.Instructions.Add(branchEndFalseInstr);
				var falseBranch = CurrentScope.EndBranch();

				branchEndTrueInstr.Branch = _currentFunction.Branches.Count;

				_currentFunction.Branches.Add(_currentFunction.Instructions.Count);
				var trueEndBranch = branchEndTrueInstr.Branch;
				var trueCombineBranchInstr = new BranchInstruction();
				_currentFunction!.Instructions.Add(trueCombineBranchInstr);

				branchEndFalseInstr.Branch = _currentFunction.Branches.Count;

				_currentFunction.Branches.Add(_currentFunction.Instructions.Count);
				var falseEndBranch = branchEndFalseInstr.Branch;
				var falseCombineBranchInstr = new BranchInstruction();
				_currentFunction!.Instructions.Add(falseCombineBranchInstr);

				if (trueCombineBranchInstr != null)
					trueCombineBranchInstr.Branch = _currentFunction.Branches.Count;

				if (falseCombineBranchInstr != null)
					falseCombineBranchInstr.Branch = _currentFunction.Branches.Count;

				_currentFunction.Branches.Add(_currentFunction.Instructions.Count);

				var phis = new Dictionary<string, PhiInstruction>();

				foreach (var (name, newReg) in trueBranch.Vars)
				{
					var oldReg = CurrentScope.GetVariable(name);
					var type = _currentFunction.Registers[oldReg];
					var phiResultReg = _currentFunction.Registers.Count;
					_currentFunction.Registers.Add(type);
					var phi = new PhiInstruction(trueEndBranch, newReg, falseEndBranch, oldReg, phiResultReg);
					phis.Add(name, phi);
					CurrentScope.ReassignVariable(name, phiResultReg);
					_currentFunction.Instructions.Add(phi);
				}

				foreach (var (name, newReg) in falseBranch.Vars)
				{
					if (phis.TryGetValue(name, out var phi))
					{
						phi.Reg2 = newReg;
					}
					else
					{
						var oldReg = CurrentScope.GetVariable(name);
						var type = _currentFunction.Registers[oldReg];
						var phiResultReg = _currentFunction.Registers.Count;
						_currentFunction.Registers.Add(type);
						phi = new PhiInstruction(trueEndBranch, oldReg, falseEndBranch, newReg, phiResultReg);
						phis.Add(name, phi);
						CurrentScope.ReassignVariable(name, phiResultReg);
						_currentFunction.Instructions.Add(phi);
					}
				}
				break;
			case ReturnStatement s:
				{
					int regIndex;
					if (s.Value == null)
					{
						regIndex = _currentFunction!.Registers.Count;
						_currentFunction.Registers.Add(new NothingIrType());
					}
					else
						regIndex = GenerateExpression(s.Value);

					_currentFunction!.Instructions.Add(new ReturnInstruction(regIndex));
					break;
				}
			case VariableDeclaration s:
				{
					var regIndex = GenerateExpression(s.Initializer);
					var varType = GetIrTypeByName(s.TypeName);

					// TOOD: Check if regIndex type and varType are different
					_currentFunction!.Registers[regIndex] = varType;

					CurrentScope!.DeclareVariable(s.Name, regIndex);
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
			case AssignmentExpression e:
				{
					var index = GenerateExpression(e.Value);
					CurrentScope!.ReassignVariable(e.Target, index);
					return index;
				}
			case BinaryExpression e:
				{
					var leftResultIndex = GenerateExpression(e.Left);
					var rightResultIndex = GenerateExpression(e.Right);

					var regIndex = _currentFunction!.Registers.Count;

					switch (e.Operation)
					{
						case BinaryOperation.Add:
							_currentFunction.Instructions.Add(new AddInstruction(leftResultIndex, rightResultIndex, regIndex));
							_currentFunction.Registers.Add(new UnknownIrType());
							break;
						case BinaryOperation.Subtract:
							_currentFunction.Instructions.Add(new SubtractInstruction(leftResultIndex, rightResultIndex, regIndex));
							_currentFunction.Registers.Add(new UnknownIrType());
							break;
						case BinaryOperation.CompareEqual:
							_currentFunction.Instructions.Add(new CompareEqualInstruction(leftResultIndex, rightResultIndex, regIndex));
							_currentFunction.Registers.Add(new BoolIrType());
							break;
						default:
							throw new NotImplementedException(e.Operation.ToString());
					}

					return regIndex;
				}
			case CallExpression e:
				{
					if (e.Target is not ReferenceExpression reference)
						throw new NotImplementedException();

					var regIndex = _currentFunction!.Registers.Count;

					_currentFunction.Registers.Add(new UnknownIrType());

					var args = new int[e.Arguments.Count];

					for (int i = 0; i < e.Arguments.Count; i++)
						args[i] = GenerateExpression(e.Arguments[i]);

					_currentFunction.Instructions.Add(new CallFunctionByNameInstruction(reference.Name, args, regIndex));

					return regIndex;
				}
			case NumericLiteralExpression e:
				{
					if (!BigInteger.TryParse(e.SourceFileFragment.Text, out var value))
						throw new($"Failed to parse numeric literal '{e.SourceFileFragment.Text}'");

					var regIndex = _currentFunction!.Registers.Count;

					_currentFunction.Instructions.Add(new StoreConstInstruction(value, regIndex));
					_currentFunction.Registers.Add(new IntegerIrType(true, defaultIntSize));

					return regIndex;
				}
			case ReferenceExpression e:
				var registerIndex = CurrentScope!.GetVariable(e.Name);
				return registerIndex;
			default:
				throw new NotImplementedException(Util.StringifyExpression(expression));
		}
	}

	private void ResolveFunctionReferences()
	{
		foreach (var func in _functions)
		{
			for (var i = 0; i < func.Instructions.Count; i++)
			{
				if (func.Instructions[i] is not CallFunctionByNameInstruction inst)
					continue;

				var funcIndex = GetFunctionIndexByName(inst.Name);
				func.Instructions[i] = new CallFunctionInstruction(funcIndex, inst.ArgRegIndices, inst.DestReg);
			}
		}
	}

	private bool InferTypes()
	{
		var changed = false;

		foreach (var func in _functions)
		{
			for (var i = 0; i < func.Instructions.Count; i++)
			{
				switch (func.Instructions[i])
				{
					case CallFunctionInstruction inst:
						{
							// Function return type
							if (func.Registers[inst.DestReg] is UnknownIrType)
							{
								func.Registers[inst.DestReg] = GetFunctionByIndex(inst.FuncIndex).ReturnType;

								changed = true;
							}
							break;
						}
					case BinaryOperationInstruction inst:
						{
							if (inst is AddInstruction or SubtractInstruction)
							{
								var leftType = func.Registers[inst.LeftSourceReg];
								var rightType = func.Registers[inst.RightSourceReg];
								var destType = func.Registers[inst.DestReg];

								if (leftType is not IntegerIrType l || rightType is not IntegerIrType r || destType is not UnknownIrType)
									break;

								var bits = Math.Max(l.Bits, r.Bits);
								var signed = l.Signed || r.Signed;

								if ((l.Bits > r.Bits && !l.Signed) || (r.Bits > l.Bits && !r.Signed))
									bits <<= 1;

								func.Registers[inst.DestReg] = new IntegerIrType(signed, bits);

								changed = true;
							}
							break;
						}
				}
			}
		}

		return changed;
	}
}
