namespace ALang.Parsing;

internal struct FunctionParameter(string name, string type)
{
	public string Name = name;
	public string Type = type;
}
