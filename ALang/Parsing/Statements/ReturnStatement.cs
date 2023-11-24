using ALang.Parsing.Expressions;

namespace ALang.Parsing.Statements;

internal sealed class ReturnStatement(SourceFileFragment sourceFileFragment, Expression? value) : Statement(sourceFileFragment)
{
	public readonly Expression? Value = value;
}
