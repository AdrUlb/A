namespace ALang.Parsing.Expressions;

internal sealed class BinaryExpression(SourceFileFragment sourceFileFragment, Expression left, Expression right, BinaryOperation operation) : Expression(sourceFileFragment)
{
	public Expression Left = left;
	public Expression Right = right;
	public BinaryOperation Operation = operation;
}
