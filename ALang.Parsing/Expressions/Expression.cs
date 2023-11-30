namespace ALang.Parsing.Expressions;

public abstract class Expression(SourceFileFragment sourceFileFragment)
{
	public readonly SourceFileFragment SourceFileFragment = sourceFileFragment;
}
