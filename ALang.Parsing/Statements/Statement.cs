namespace ALang.Parsing.Statements;

public abstract class Statement(SourceFileFragment sourceFileFragment)
{
	public readonly SourceFileFragment SourceFileFragment = sourceFileFragment;
}
