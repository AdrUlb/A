namespace ALang.Parsing.Expressions;

internal class AssignmentExpression(SourceFileFragment sourceFileFragment, string target, Expression value) : Expression(sourceFileFragment)
{
	public readonly string Target = target;
	public readonly Expression Value = value;
}
