using ALang.Parsing.Expressions;

namespace ALang.Parsing.Statements;

internal sealed class IfStatement(SourceFileFragment sourceFileFragment, Expression condition, Statement body) : Statement(sourceFileFragment)
{
	public readonly Expression Condition = condition;
	public readonly Statement Body = body;
}
