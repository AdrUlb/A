namespace ALang.Lexing.Tokens;

public sealed class CommentToken(SourceFileFragment sourceFileFragment) : Token(sourceFileFragment);
