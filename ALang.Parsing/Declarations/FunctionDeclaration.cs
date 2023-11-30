using ALang.Parsing.Statements;

namespace ALang.Parsing.Declarations;

public sealed class FunctionDeclaration(
	SourceFileFragment sourceFileFragment,
	string name,
	IReadOnlyList<FunctionParameter> parameters,
	string? returnType,
	Statement body) : Statement(sourceFileFragment)
{
	public readonly string Name = name;
	public readonly IReadOnlyList<FunctionParameter> Parameters = parameters;
	public readonly string? ReturnType = returnType;
	public readonly Statement Body = body;
}
