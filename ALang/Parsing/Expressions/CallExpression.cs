namespace ALang.Parsing.Expressions;

internal sealed class CallExpression(SourceFileFragment sourceFileFragment, Expression expression, IReadOnlyList<Expression> arguments)
	: Expression(sourceFileFragment)
{
	public readonly Expression Expression = expression;
	public readonly IReadOnlyList<Expression> Arguments = arguments;
}
