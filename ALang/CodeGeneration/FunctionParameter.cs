namespace ALang.CodeGeneration;

internal sealed class FunctionParameter(string name, LangType type)
{
	public readonly string Name = name;
	public readonly LangType Type = type;
}
