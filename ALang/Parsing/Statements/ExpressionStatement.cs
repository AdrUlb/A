using ALang.Parsing.Expressions;

namespace ALang.Parsing.Statements;

internal sealed class ExpressionStatement(SourceFileFragment sourceFileFragment, Expression expression) : Statement(sourceFileFragment)
{
	public readonly Expression Expression = expression;
}
