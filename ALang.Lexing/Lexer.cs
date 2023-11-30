using ALang.Lexing.Tokens;
using System.Collections.Frozen;
using System.Text;

namespace ALang.Lexing;

public sealed class Lexer(SourceFile file)
{
	private int _index = 0;
	private int _line = 1;
	private int _column = 1;

	private static readonly FrozenDictionary<string, KeywordType> _keywords = new Dictionary<string, KeywordType>
	{
		{ "func", KeywordType.Func },
		{ "return", KeywordType.Return },
		{ "var", KeywordType.Var },
		{ "if", KeywordType.If },
	}.ToFrozenDictionary();

	private static readonly FrozenDictionary<string, PunctuatorType> _punctuators = new Dictionary<string, PunctuatorType>
	{
		{ "(", PunctuatorType.ParenOpen },
		{ ")", PunctuatorType.ParenClose },
		{ "{", PunctuatorType.CurlyOpen },
		{ "}", PunctuatorType.CurlyClose },
		{ "+", PunctuatorType.Plus },
		{ "-", PunctuatorType.Minus },
		{ "*", PunctuatorType.Asterisk },
		{ "/", PunctuatorType.Slash },
		{ "_", PunctuatorType.Underscore },
		{ "=", PunctuatorType.Equal },
		{ ",", PunctuatorType.Comma },
		{ ":", PunctuatorType.Colon },
		{ ";", PunctuatorType.Semicolon },
		{ "==", PunctuatorType.EqualEqual },
		{ "!=", PunctuatorType.BangEqual }
	}.ToFrozenDictionary();

	private bool TryPeekChar(out char @char, out int index, out int line, out int column)
	{
		index = _index;
		line = _line;
		column = _column;

		if (_index >= file.Contents.Length)
		{
			@char = '\0';
			return false;
		}

		@char = file.Contents[index];

		return true;
	}

	private bool TryReadChar(out char @char, out int index, out int line, out int column)
	{
		if (!TryPeekChar(out @char, out index, out line, out column))
			return false;

		_index++;

		if (@char == '\n')
		{
			_line++;
			_column = 1;
		}
		else
			_column++;

		return true;
	}

	private bool TrySkipChar() => TryReadChar(out _, out _, out _, out _);

	public List<Token> Analyze()
	{
		var tokens = new List<Token>();

		while (TryReadChar(out var @char, out var index, out var line, out var column))
		{
			switch (@char)
			{
				case ' ' or '\t' or '\n' or '\r':
					{
						int end;
						while (TryPeekChar(out @char, out end, out _, out _) && @char is ' ' or '\t' or '\n' or '\r')
							TrySkipChar();

						tokens.Add(new WhitespaceToken(new(file, index, line, column, end - index)));
						break;
					}
				case (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_':
					{
						int end;
						while (TryPeekChar(out @char, out end, out _, out _) &&
							   @char is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_' or (>= '0' and <= '9'))
							TrySkipChar();

						var fragment = new SourceFileFragment(file, index, line, column, end - index);

						if (_keywords.TryGetValue(fragment.Text, out var keywordType))
						{
							tokens.Add(new KeywordToken(fragment, keywordType));
							break;
						}

						tokens.Add(new IdentifierToken(fragment));
						break;
					}
				case >= '0' and <= '9':
					{
						int end;
						while (TryPeekChar(out @char, out end, out _, out _) && @char is >= '0' and <= '9')
							TrySkipChar();

						tokens.Add(new NumericLiteralToken(new(file, index, line, column, end - index)));
						break;
					}
				case '/':
					{
						if (!TryPeekChar(out @char, out _, out _, out _))
							goto default;

						if (@char == '/')
						{
							TrySkipChar();

							int end;
							while (TryPeekChar(out @char, out end, out _, out _) && @char is not '\n')
								TrySkipChar();

							if (TrySkipChar())
								end++;

							tokens.Add(new CommentToken(new(file, index, line, column, end - index)));

							break;
						}

						if (@char == '*')
						{
							TrySkipChar();

							var lastChar = '\0';

							while (true)
							{
								if (!TryPeekChar(out @char, out var end, out var l, out var c))
								{
									tokens.Add(new CommentToken(new(file, index, line, column, end - index)));
									tokens.Add(new ErrorToken(new(file, end, l, c, 1), "Unexpected end of file"));
									break;
								}

								TrySkipChar();

								if (lastChar == '*' && @char == '/')
								{
									tokens.Add(new CommentToken(new(file, index, line, column, end - index + 1)));
									break;
								}

								lastChar = @char;
							}

							break;
						}

						goto default;
					}
				default:
					{
						var sb = new StringBuilder().Append(@char);

						if (_punctuators.Keys.Any(k => k.StartsWith(sb.ToString())))
						{
							while (true)
							{
								if (!TryPeekChar(out @char, out _, out _, out _))
									break;

								sb.Append(@char);

								if (_punctuators.Keys.Any(k => k.StartsWith(sb.ToString())))
								{
									TrySkipChar();
									continue;
								}

								sb.Length--;
								break;
							}

							if (_punctuators.TryGetValue(sb.ToString(), out var punctuatorType))
							{
								tokens.Add(new PunctuatorToken(new(file, index, line, column, sb.Length), punctuatorType));
								break;
							}
						}

						tokens.Add(new ErrorToken(new(file, index, line, column, 1), $"Unexpected character '{@char}'"));
						break;
					}
			}
		}

		return tokens;
	}
}
