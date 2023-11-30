using System.Diagnostics;

namespace ALang;

public struct SourceFileFragment(SourceFile file, int index, int line, int column, int length)
{
	public SourceFile File = file;
	public int Index = index;
	public int Line = line;
	public int Column = column;
	public int Length = length;
	
	public readonly string Text = index + length <= file.Contents.Length ? file.Contents[index..(index + length)] : " ";

	public SourceFileFragment() : this(null!, 0, 0, 0, 0) =>
		throw new UnreachableException($"Parameterless constructor of '{nameof(SourceFileFragment)}' was called.");
}
