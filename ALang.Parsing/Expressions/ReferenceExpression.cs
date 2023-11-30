namespace ALang.Parsing.Expressions;

public sealed class ReferenceExpression(SourceFileFragment sourceFileFragment, string value) : Expression(sourceFileFragment)
{
	public readonly string Name = value;
}
