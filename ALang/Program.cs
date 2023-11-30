using ALang;
using ALang.Ir;
using ALang.Lexing;
using ALang.Parsing;

switch (args.Length)
{
	case 0:
		RunRepl();
		break;
	case 1:
		RunFile(args[0]);
		break;
	default:
		Console.WriteLine($"usage: alang [file]");
		break;
}

return;

static void RunRepl()
{
	while (true)
	{
		Console.Write("> ");
		var input = Console.ReadLine();
	}
}

static void RunFile(string filePath)
{
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

	var irGen = new IrGenerator(statements);
	irGen.Generate();
}
