using ALang.Parsing.Expressions;
using ALang.Parsing.Statements;

namespace ALang.Parsing.Declarations;

internal sealed class VariableDeclaration(SourceFileFragment sourceFileFragment, string name, string type, Expression initializer) : Statement(sourceFileFragment)
{
	public readonly string Name = name;
	public readonly string Type = type;
	public readonly Expression Initializer = initializer;
}
