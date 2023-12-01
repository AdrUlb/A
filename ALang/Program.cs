using ALang;
using ALang.Ir;
using ALang.Lexing;
using ALang.Parsing;
using System.Text;

RunFile(args[0]);

static void RunFile(string filePath)
{
	ulong a = 10;
	uint b = 20;
	var c = b - a;

	var sourceFile = new SourceFile(filePath, File.ReadAllText(filePath));

	var lexer = new Lexer(sourceFile);
	var tokens = lexer.Analyze();

	/*foreach (var token in tokens)
	{
		if (token is WhitespaceToken or CommentToken)
			continue;
		Console.WriteLine(Util.StringifyToken(token));
	}*/

	var parser = new Parser(tokens);
	var statements = parser.Parse();

	/*foreach (var statement in statements)
		Console.WriteLine(Util.StringifyStatement(statement));*/

	var irGen = new IrGenerator();
	irGen.Generate(statements);
	var functions = irGen._functions;

	for (int i = 0; i < functions.Count; i++)
	{
		IrFunction? f = functions[i];
		Console.WriteLine($"{i}: {f.ReturnType} {f.Name}({string.Join(", ", f.Parameters.Select(p => p.Type))}):");

		var localsStringBuilder = new StringBuilder(".regs ");

		for (var j = 0; j < f.Registers.Count; j++)
		{
			if (j != 0)
				localsStringBuilder.Append(", ");

			localsStringBuilder.Append(j).Append(':').Append(f.Registers[j].ToString());
		}

		Console.WriteLine(localsStringBuilder);

		for (int j = 0; j < f.Instructions.Count; j++)
		{
			IrInstruction instr = f.Instructions[j];
			switch (instr)
			{
				case CompareEqualInstruction inst:
					Console.WriteLine($"0x{j:X4}: reg{inst.DestReg} <- reg{inst.LeftSourceReg} == reg{inst.RightSourceReg}");
					break;
				case JumpIfFalseInstruction inst:
					Console.WriteLine($"0x{j:X4}: jmpfalse 0x{inst.JumpOffset:X4}, reg{inst.SourceReg}");
					break;
				case CallFunctionInstruction inst:
					Console.WriteLine($"0x{j:X4}: reg{inst.DestReg} <- call func{inst.FuncIndex}({string.Join(", ", inst.ArgRegIndices.Select(i => $"reg{i}"))})");
					break;
				case CallFunctionByNameInstruction inst:
					Console.WriteLine($"0x{j:X4}: reg{inst.DestReg} <- call {inst.Name}({string.Join(", ", inst.ArgRegIndices.Select(i => $"reg{i}"))})");
					break;
				case ReturnInstruction inst:
					Console.WriteLine($"0x{j:X4}: ret reg{inst.SourceReg}");
					break;
				case StoreArgumentInstruction inst:
					Console.WriteLine($"0x{j:X4}: reg{inst.DestReg} <- arg{inst.ArgumentIndex}");
					break;
				case StoreConstInstruction inst:
					Console.WriteLine($"0x{j:X4}: reg{inst.DestReg} <- {inst.Value}");
					break;
				case AddInstruction inst:
					Console.WriteLine($"0x{j:X4}: reg{inst.DestReg} <- reg{inst.LeftSourceReg} - reg{inst.RightSourceReg}");
					break;
				case SubtractInstruction inst:
					Console.WriteLine($"0x{j:X4}: reg{inst.DestReg} <- reg{inst.LeftSourceReg} - reg{inst.RightSourceReg}");
					break;
				default:
					throw new(instr.GetType().Name);
			}
		}
	}
}
