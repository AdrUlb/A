namespace ALang.Lexing.Tokens;

public sealed class PunctuatorToken(SourceFileFragment sourceFileFragment, PunctuatorType type) : Token(sourceFileFragment)
{
	public readonly PunctuatorType Type = type;
}
