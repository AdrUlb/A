namespace ALang.Parsing.Expressions;

public sealed class CallExpression(SourceFileFragment sourceFileFragment, Expression expression, IReadOnlyList<Expression> arguments)
	: Expression(sourceFileFragment)
{
	public readonly Expression Target = expression;
	public readonly IReadOnlyList<Expression> Arguments = arguments;
}
