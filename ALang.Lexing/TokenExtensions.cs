using ALang.Lexing.Tokens;
using System.Text;

namespace ALang.Lexing;

public static class TokenExtensions
{
	public static string Stringify(this Token token)
	{
		var builder = new StringBuilder().Append(token.GetType().Name).AppendLine(" {")
			.Append(new string(' ', 4)).Append("Lexeme: \"").Append(token.Lexeme).Append('"')
			.AppendLine(",").Append(new string(' ', 4)).Append("Line: ").Append(token.SourceFileFragment.Line)
			.AppendLine(",").Append(new string(' ', 4)).Append("Column: ").Append(token.SourceFileFragment.Column);

		switch (token)
		{
			case ErrorToken t:
				builder.AppendLine(",").Append(new string(' ', 4)).Append("Message: \"").Append(t.Message).Append('"');
				break;
			case KeywordToken t:
				builder.AppendLine(",").Append(new string(' ', 4)).Append("Type: ").Append(t.Type);
				break;
			case PunctuatorToken t:
				builder.AppendLine(",").Append(new string(' ', 4)).Append("Type: ").Append(t.Type);
				break;
		}

		return builder.AppendLine().Append('}').ToString();
	}
}
