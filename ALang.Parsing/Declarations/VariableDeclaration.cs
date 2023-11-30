using ALang.Parsing.Expressions;
using ALang.Parsing.Statements;

namespace ALang.Parsing.Declarations;

public sealed class VariableDeclaration(SourceFileFragment sourceFileFragment, string name, string type, Expression initializer) : Statement(sourceFileFragment)
{
	public readonly string Name = name;
	public readonly string TypeName = type;
	public readonly Expression Initializer = initializer;
}
