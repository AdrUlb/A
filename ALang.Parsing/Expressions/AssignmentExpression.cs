namespace ALang.Parsing.Expressions;

public class AssignmentExpression(SourceFileFragment sourceFileFragment, string target, Expression value) : Expression(sourceFileFragment)
{
	public readonly string Target = target;
	public readonly Expression Value = value;
}
