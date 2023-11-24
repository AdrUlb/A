namespace ALang.Lexing.Tokens;

internal sealed class KeywordToken(SourceFileFragment sourceFileFragment, KeywordType type) : Token(sourceFileFragment)
{
	public readonly KeywordType Type = type;
}
