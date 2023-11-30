namespace ALang.Lexing.Tokens;

public abstract class Token(SourceFileFragment sourceFileFragment)
{
	public readonly SourceFileFragment SourceFileFragment = sourceFileFragment;
	
	public string Lexeme => SourceFileFragment.Text;
}
