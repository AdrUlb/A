namespace ALang.Parsing.Statements;

internal abstract class Statement(SourceFileFragment sourceFileFragment)
{
	public readonly SourceFileFragment SourceFileFragment = sourceFileFragment;
}
