namespace ALang;

internal sealed class SourceFile(string name, string content)
{
	public readonly string Name = name;
	public readonly string Contents = content;
}
