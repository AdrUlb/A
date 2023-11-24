namespace ALang.Parsing.Expressions;

internal abstract class Expression(SourceFileFragment sourceFileFragment)
{
	public readonly SourceFileFragment SourceFileFragment = sourceFileFragment;
}
