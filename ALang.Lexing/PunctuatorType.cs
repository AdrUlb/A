namespace ALang.Lexing;

public enum PunctuatorType
{
	Invalid,
	ParenOpen, ParenClose,
	CurlyOpen, CurlyClose,
	Plus, Minus, Asterisk, Slash,
	Underscore,
	Equal, EqualEqual, BangEqual,
	Comma,
	Colon, Semicolon
}
