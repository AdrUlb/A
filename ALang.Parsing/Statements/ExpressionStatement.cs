using ALang.Parsing.Expressions;

namespace ALang.Parsing.Statements;

public sealed class ExpressionStatement(SourceFileFragment sourceFileFragment, Expression expression) : Statement(sourceFileFragment)
{
	public readonly Expression Expression = expression;
}
