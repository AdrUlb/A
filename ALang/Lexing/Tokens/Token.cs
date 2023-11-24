namespace ALang.Lexing.Tokens;

internal abstract class Token(SourceFileFragment sourceFileFragment)
{
	public readonly SourceFileFragment SourceFileFragment = sourceFileFragment;
	
	public string Lexeme => SourceFileFragment.Text;
}
