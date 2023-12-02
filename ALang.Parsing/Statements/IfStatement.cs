using ALang.Parsing.Expressions;

namespace ALang.Parsing.Statements;

public sealed class IfStatement(SourceFileFragment sourceFileFragment, Expression condition, Statement body, Statement? elseBody = null) : Statement(sourceFileFragment)
{
	public readonly Expression Condition = condition;
	public readonly Statement Body = body;
	public readonly Statement? ElseBody = elseBody;
}
