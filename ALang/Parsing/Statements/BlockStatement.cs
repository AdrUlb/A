namespace ALang.Parsing.Statements;

internal sealed class BlockStatement(SourceFileFragment sourceFileFragment, IReadOnlyList<Statement> statements) : Statement(sourceFileFragment)
{
	public readonly IReadOnlyList<Statement> Statements = statements;
}
