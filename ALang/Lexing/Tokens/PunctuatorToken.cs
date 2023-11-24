namespace ALang.Lexing.Tokens;

internal sealed class PunctuatorToken(SourceFileFragment sourceFileFragment, PunctuatorType type) : Token(sourceFileFragment)
{
	public readonly PunctuatorType Type = type;
}
