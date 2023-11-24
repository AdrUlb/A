namespace ALang.Lexing.Tokens;

internal sealed class ErrorToken(SourceFileFragment sourceFileFragment, string message) : Token(sourceFileFragment)
{
	public readonly string Message = message;
}
