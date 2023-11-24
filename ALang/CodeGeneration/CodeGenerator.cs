using ALang.Parsing;
using ALang.Parsing.Declarations;
using ALang.Parsing.Expressions;
using ALang.Parsing.Statements;

namespace ALang.CodeGeneration;

internal sealed class CodeGenerator(IReadOnlyList<Statement> statements)
{
	private readonly Stack<Function> _functions = new();

	private Function CurrentFunction => _functions.Peek();

	private int EmitInstruction(Instruction instruction)
	{
		var i = CurrentFunction.Code.Count;
		CurrentFunction.Code.Add((byte)instruction);
		return i;
	}

	private int EmitU32(uint value)
	{
		var i = CurrentFunction.Code.Count;
		CurrentFunction.Code.Add(value);
		return i;
	}

	private static bool IsIntType(LangType type) => type is LangType.U8 or LangType.U16;

	private static LangType GetLangTypeFromName(string? name)
	{
		return name switch
		{
			"u16" => LangType.U16,
			null => LangType.Nothing,
			_ => throw new($"Unknown type '{name}'")
		};
	}

	public void Generate()
	{
		foreach (var statement in statements)
		{
			switch (statement)
			{
				case FunctionDeclaration s:
					{
						var types = new FunctionParameter[s.Parameters.Count];

						for (var i = 0; i < s.Parameters.Count; i++)
						{
							var param = s.Parameters[i];
							types[i] = new(param.Name, GetLangTypeFromName(param.Type));
						}

						_functions.Push(new(s.Name, types, GetLangTypeFromName(s.ReturnType)));

						GenerateStatement(s.Body);

						break;
					}
				default:
					throw new($"Cannot generate {statement}");
			}
		}
	}

	private void GenerateStatement(Statement statement)
	{
		switch (statement)
		{
			case BlockStatement s:
				foreach (var stmt in s.Statements)
				{
					GenerateStatement(stmt);
				}
				break;
			case IfStatement s:
				{
					var condExprType = GenerateExpression(s.Condition);

					if (condExprType is not LangType.Bool)
						throw new();

					EmitInstruction(Instruction.JumpFalse);
					var jumpTargetIndex = EmitU32(0);

					GenerateStatement(s.Body);
					CurrentFunction.Code[jumpTargetIndex] = (uint)CurrentFunction.Code.Count;

					break;
				}
			default:
				throw new($"Cannot generate {statement}");
		}
	}

	private LangType GenerateExpression(Expression expression)
	{
		switch (expression)
		{
			case BinaryExpression e:
				switch (e.Operation)
				{
					case BinaryOperation.CompareEqual:
						GenerateExpression(e.Left);
						GenerateExpression(e.Right);
						EmitInstruction(Instruction.CompareEqual);
						return LangType.Bool;
					default:
						throw new NotImplementedException($"Binary operation {e.Operation} not implemented.");
				}
			case ReferenceExpression e:
				if (CurrentFunction.TryGetParameterIndex(e.Name, out var index))
				{
					EmitInstruction(Instruction.);
					break;
				}
				break;
			default:
				throw new NotImplementedException($"Cannot generate {expression}");
		}
	}
}
