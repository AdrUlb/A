using ALang.Lexing;
using ALang.Lexing.Tokens;
using ALang.Parsing.Declarations;
using ALang.Parsing.Expressions;
using ALang.Parsing.Statements;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ALang.Parsing;

public sealed class Parser(IReadOnlyList<Token> tokens)
{
	private int _index = 0;

	private bool TryPeekToken([MaybeNullWhen(false)] out Token token)
	{
		if (_index >= tokens.Count)
		{
			token = null;
			return false;
		}

		token = tokens[_index];

		while (token is CommentToken or WhitespaceToken)
		{
			if (++_index >= tokens.Count)
				return false;

			token = tokens[_index];
		}

		return true;
	}

	private bool TryReadToken([MaybeNullWhen(false)] out Token token)
	{
		if (!TryPeekToken(out token))
			return false;

		_index++;

		return true;
	}

	private readonly Stack<int> _indexStack = new();

	private bool TrySkipToken() => TryReadToken(out _);

	private bool IsEndReached => !TryPeekToken(out _);

	private static SourceFileFragment CreateFragment(SourceFileFragment startFragment, SourceFileFragment endFragment) =>
		startFragment with { Length = endFragment.Length - startFragment.Length + 1 };

	private static string GenerateExpectedFoundString(string expected, Token? found) =>
		found == null
			? $"Expected {expected}, found end-of-file"
			: $"Expected {expected}, found {found.Stringify()}";

	public List<Statement> Parse()
	{
		var statements = new List<Statement>();

		while (!IsEndReached)
		{
			var statement = ParseDeclaration();
			statements.Add(statement);
		}

		return statements;
	}

	private Statement ParseDeclaration()
	{
		if (!TryPeekToken(out var token))
			throw new(GenerateExpectedFoundString("declaration", token));

		return token switch
		{
			KeywordToken { Type: KeywordType.Func } => ParseFunctionDeclaration(),
			KeywordToken { Type: KeywordType.Var } => ParseVariableDeclaration(),
			_ => ParseStatement()
		};
	}

	private FunctionDeclaration ParseFunctionDeclaration()
	{
		if (!TryReadToken(out var token) || token is not KeywordToken { Type: KeywordType.Func })
			throw new(GenerateExpectedFoundString("'func'", token));

		var startFragment = token.SourceFileFragment;

		if (!TryReadToken(out token) || token is not IdentifierToken nameIdentifier)
			throw new(GenerateExpectedFoundString("identifier", token));

		var name = nameIdentifier.Lexeme;

		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.ParenOpen })
			throw new(GenerateExpectedFoundString("'('", token));

		var parameters = new List<FunctionParameter>();

		if (TryPeekToken(out token) && token is IdentifierToken)
		{
			while (true)
			{
				if (!TryPeekToken(out token) || token is not IdentifierToken parameterNameIdentifier)
					throw new(GenerateExpectedFoundString("identifier", token));

				TrySkipToken();

				if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.Colon })
					throw new(GenerateExpectedFoundString("':'", token));

				if (!TryReadToken(out token) || token is not IdentifierToken parameterTypeIdentifier)
					throw new(GenerateExpectedFoundString("identifier", token));

				parameters.Add(new(parameterNameIdentifier.Lexeme, parameterTypeIdentifier.Lexeme));

				if (!TryPeekToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.Comma })
					break;

				TrySkipToken();
			}
		}

		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.ParenClose })
			throw new(GenerateExpectedFoundString("')'", token));

		string? returnType = null;

		if (TryPeekToken(out token) && token is PunctuatorToken { Type: PunctuatorType.Colon })
		{
			TrySkipToken();

			if (!TryReadToken(out token) || token is not IdentifierToken returnTypeIdentifier)
				throw new(GenerateExpectedFoundString("identifier", token));

			returnType = returnTypeIdentifier.Lexeme;
		}

		if (ParseStatement() is not { } body)
			throw new(GenerateExpectedFoundString("statement", token));

		var endFragment = body.SourceFileFragment;

		return new(CreateFragment(startFragment, endFragment), name, parameters, returnType, body);
	}

	private VariableDeclaration ParseVariableDeclaration()
	{
		if (!TryReadToken(out var token) || token is not KeywordToken { Type: KeywordType.Var })
			throw new(GenerateExpectedFoundString("'var'", token));

		var startFragment = token.SourceFileFragment;

		if (!TryReadToken(out token) || token is not IdentifierToken nameIdentifier)
			throw new(GenerateExpectedFoundString("identifier", token));

		var name = nameIdentifier.Lexeme;
		
		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.Colon })
			throw new(GenerateExpectedFoundString("':'", token));

		if (!TryReadToken(out token) || token is not IdentifierToken typeIdentifier)
			throw new(GenerateExpectedFoundString("identifier", token));

		var type = typeIdentifier.Lexeme;

		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.Equal })
			throw new(GenerateExpectedFoundString("'='", token));

		var initializer = ParseExpression();

		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.Semicolon })
			throw new(GenerateExpectedFoundString("';'", token));

		var endFragment = token.SourceFileFragment;

		return new(CreateFragment(startFragment, endFragment), name, type, initializer);
	}

	private Statement ParseStatement()
	{
		if (!TryPeekToken(out var token))
			throw new(GenerateExpectedFoundString("statement", token));

		return token switch
		{
			KeywordToken { Type: KeywordType.Return } => ParseReturnStatement(),
			KeywordToken { Type: KeywordType.If } => ParseIfStatement(),
			PunctuatorToken { Type: PunctuatorType.CurlyOpen } => ParseBlockStatement(),
			_ => ParseExpressionStatement()
		};
	}

	private ReturnStatement ParseReturnStatement()
	{
		if (!TryReadToken(out var token) || token is not KeywordToken { Type: KeywordType.Return })
			throw new(GenerateExpectedFoundString("'return'", token));

		var startFragment = token.SourceFileFragment;

		if (!TryPeekToken(out token))
			throw new(GenerateExpectedFoundString("';' or expression", token));

		if (token is PunctuatorToken { Type: PunctuatorType.Semicolon })
		{
			TrySkipToken();

			var endFragment = token.SourceFileFragment;

			return new(CreateFragment(startFragment, endFragment), null);
		}

		var value = ParseExpression();

		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.Semicolon })
			throw new(GenerateExpectedFoundString("';'", token));

		{
			var endFragment = token.SourceFileFragment;

			return new(CreateFragment(startFragment, endFragment), value);
		}
	}

	private IfStatement ParseIfStatement()
	{
		if (!TryReadToken(out var token) || token is not KeywordToken { Type: KeywordType.If })
			throw new(GenerateExpectedFoundString("'if'", token));

		var startFragment = token.SourceFileFragment;

		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.ParenOpen })
			throw new(GenerateExpectedFoundString("'('", token));

		var condition = ParseExpression();

		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.ParenClose })
			throw new(GenerateExpectedFoundString("')'", token));

		var body = ParseStatement();

		var endFragment = body.SourceFileFragment;

		if (TryPeekToken(out token) && token is KeywordToken { Type: KeywordType.Else })
		{
			TrySkipToken();
			var elseBody = ParseStatement();
			endFragment = elseBody.SourceFileFragment;
			return new(CreateFragment(startFragment, endFragment), condition, body, elseBody);
		}

		return new(CreateFragment(startFragment, endFragment), condition, body);
	}

	private BlockStatement ParseBlockStatement()
	{
		if (!TryReadToken(out var token) || token is not PunctuatorToken { Type: PunctuatorType.CurlyOpen })
			throw new(GenerateExpectedFoundString("'{'", token));

		var startFragment = token.SourceFileFragment;

		var statements = new List<Statement>();

		while (true)
		{
			if (!TryPeekToken(out token))
				throw new(GenerateExpectedFoundString("'}' or declaration", token));

			if (token is PunctuatorToken { Type: PunctuatorType.CurlyClose })
			{
				TrySkipToken();
				break;
			}

			statements.Add(ParseDeclaration());
		}

		var endFragment = token.SourceFileFragment;

		return new(CreateFragment(startFragment, endFragment), statements);
	}

	private ExpressionStatement ParseExpressionStatement()
	{
		var expression = ParseExpression();

		var startFragment = expression.SourceFileFragment;

		if (!TryReadToken(out var token) || token is not PunctuatorToken { Type: PunctuatorType.Semicolon })
			throw new(GenerateExpectedFoundString("';'", token));

		var endFragment = token.SourceFileFragment;

		return new(CreateFragment(startFragment, endFragment), expression);
	}

	private Expression ParseExpression() => ParseAssignmentExpression();

	private Expression ParseAssignmentExpression()
	{
		// Store current location
		_indexStack.Push(_index);

		// Not an identifier? Not an assignment: restore index position.
		if (!TryReadToken(out var token) || token is not IdentifierToken identifier)
		{
			_index = _indexStack.Pop();
			return ParseEqualityExpression();
		}

		var startFragment = token.SourceFileFragment;

		// Not an equal sign? Not an assignment: restore index position.
		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.Equal })
		{
			_index = _indexStack.Pop();
			return ParseEqualityExpression();
		}

		var expr = ParseExpression();

		var endFragment = expr.SourceFileFragment;

		return new AssignmentExpression(CreateFragment(startFragment, endFragment), identifier.Lexeme, expr);
	}

	private Expression ParseEqualityExpression()
	{
		var left = ParseAddSubExpression();

		var startFragment = left.SourceFileFragment;

		while (TryPeekToken(out var token) && token is PunctuatorToken { Type: PunctuatorType.EqualEqual or PunctuatorType.BangEqual } punctuator)
		{
			TrySkipToken();

			var operation = punctuator.Type switch
			{
				PunctuatorType.EqualEqual => BinaryOperation.CompareEqual,
				PunctuatorType.BangEqual => BinaryOperation.CompareNotEqual,
				_ => throw new UnreachableException()
			};

			var right = ParseCallExpression();

			var endFragment = right.SourceFileFragment;

			left = new BinaryExpression(CreateFragment(startFragment, endFragment), left, right, operation);
		}

		return left;
	}

	private Expression ParseAddSubExpression()
	{
		var left = ParseCallExpression();

		var startFragment = left.SourceFileFragment;

		while (TryPeekToken(out var token) && token is PunctuatorToken { Type: PunctuatorType.Plus or PunctuatorType.Minus } punctuator)
		{
			TrySkipToken();

			var operation = punctuator.Type switch
			{
				PunctuatorType.Plus => BinaryOperation.Add,
				PunctuatorType.Minus => BinaryOperation.Subtract,
				_ => throw new UnreachableException()
			};

			var right = ParseCallExpression();

			var endFragment = right.SourceFileFragment;

			left = new BinaryExpression(CreateFragment(startFragment, endFragment), left, right, operation);
		}

		return left;
	}

	private Expression ParseCallExpression()
	{
		var expression = ParsePrimaryExpression();

		if (!TryPeekToken(out var token) || token is not PunctuatorToken { Type: PunctuatorType.ParenOpen })
			return expression;

		var startFragment = token.SourceFileFragment;

		TrySkipToken();

		var arguments = new List<Expression>();

		if (TryPeekToken(out token) && token is not PunctuatorToken { Type: PunctuatorType.ParenClose })
		{
			while (true)
			{
				var argument = ParseExpression();

				arguments.Add(argument);

				if (!TryPeekToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.Comma })
					break;

				TrySkipToken();
			}
		}

		if (!TryReadToken(out token) || token is not PunctuatorToken { Type: PunctuatorType.ParenClose })
			throw new(GenerateExpectedFoundString("')'", token));

		var endFragment = token.SourceFileFragment;

		return new CallExpression(CreateFragment(startFragment, endFragment), expression, arguments);
	}

	private Expression ParsePrimaryExpression()
	{
		if (!TryReadToken(out var token))
			throw new(GenerateExpectedFoundString("expression", token));

		return token switch
		{
			IdentifierToken identifier => new ReferenceExpression(identifier.SourceFileFragment, identifier.Lexeme),
			NumericLiteralToken numericLiteral => new NumericLiteralExpression(numericLiteral.SourceFileFragment),
			_ => throw new(GenerateExpectedFoundString("expression", token))
		};
	}
}
