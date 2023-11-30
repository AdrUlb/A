namespace ALang.Lexing.Tokens;

public sealed class KeywordToken(SourceFileFragment sourceFileFragment, KeywordType type) : Token(sourceFileFragment)
{
	public readonly KeywordType Type = type;
}
