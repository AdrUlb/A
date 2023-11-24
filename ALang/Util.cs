using ALang.Lexing.Tokens;
using ALang.Parsing.Declarations;
using ALang.Parsing.Expressions;
using ALang.Parsing.Statements;
using System.Text;

namespace ALang;

internal static class Util
{
	public static string Escape(this string value)
	{
		return value
			.Replace("\n", "\\n")
			.Replace("\t", "\\t");
	}

	public static string StringifyToken(Token token)
	{
		var builder = new StringBuilder().Append(token.GetType().Name).AppendLine(" {")
			.Append(new string(' ', 4)).Append("Lexeme: \"").Append(token.Lexeme.Escape()).Append('"')
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

	public static string StringifyStatement(Statement statement, int indentLevel = 0)
	{
		indentLevel++;
		var builder = new StringBuilder()
			.Append(statement.GetType().Name).AppendLine(" {")
			.Append(new string(' ', indentLevel * 4)).Append("Line: ").Append(statement.SourceFileFragment.Line)
			.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Column: ").Append(statement.SourceFileFragment.Column);

		switch (statement)
		{
			case FunctionDeclaration s:
				builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Name: \"").Append(s.Name).Append('"');

				if (s.Parameters.Count != 0)
				{
					builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).AppendLine("Parameters: [");

					indentLevel++;

					for (var i = 0; i < s.Parameters.Count; i++)
					{
						if (i != 0)
							builder.AppendLine(",");

						builder
							.Append(new string(' ', indentLevel * 4)).AppendLine("Parameter {");

						indentLevel++;

						builder.Append(new string(' ', indentLevel * 4)).Append("Name: \"").Append(s.Parameters[i].Name.Escape()).AppendLine("\",");
						builder.Append(new string(' ', indentLevel * 4)).Append("Type: \"").Append(s.Parameters[i].Type.Escape()).AppendLine("\"");

						indentLevel--;

						builder.Append(new string(' ', indentLevel * 4)).Append('}');
					}

					indentLevel--;

					builder.AppendLine().Append(new string(' ', indentLevel * 4)).Append(']');
				}
				else
					builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Parameters: []");

				builder
					.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Body: ").Append(StringifyStatement(s.Body, indentLevel))
					.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Return type: ");

				if (s.ReturnType == null)
					builder.Append("none");
				else
					builder.Append('\"').Append(s.ReturnType.Escape()).Append('\"');

				break;
			case VariableDeclaration s:
			{
				builder
					.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Name: \"").Append(s.Name).Append('"')
					.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Type: \"").Append(s.Type).Append('"')
					.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Initializer: ").Append(StringifyExpression(s.Initializer, indentLevel));

				break;
			}
			case BlockStatement s:
				if (s.Statements.Count != 0)
				{
					builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).AppendLine("Statements: [");

					indentLevel++;

					for (var i = 0; i < s.Statements.Count; i++)
					{
						if (i != 0)
							builder.AppendLine(",");

						builder.Append(new string(' ', indentLevel * 4)).Append(StringifyStatement(s.Statements[i], indentLevel));
					}

					indentLevel--;

					builder.AppendLine().Append(new string(' ', indentLevel * 4)).Append(']');
				}
				else
					builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Statements: []");

				break;
			case ExpressionStatement s:
				builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Expression: ").Append(StringifyExpression(s.Expression, indentLevel));
				break;
			case ReturnStatement s:
				builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Value: ").Append(StringifyExpression(s.Value, indentLevel));
				break;
		}

		indentLevel--;

		return builder.AppendLine().Append(new string(' ', indentLevel * 4)).Append('}').ToString();
	}

	public static string StringifyExpression(Expression? expression, int indentLevel = 0)
	{
		if (expression == null)
			return "none";

		indentLevel++;
		var builder = new StringBuilder()
			.Append(expression.GetType().Name).AppendLine(" {")
			.Append(new string(' ', indentLevel * 4)).Append("Line: ").Append(expression.SourceFileFragment.Line)
			.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Column: ").Append(expression.SourceFileFragment.Column);

		switch (expression)
		{
			case BinaryExpression e:
				builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Left: ").Append(StringifyExpression(e.Left, indentLevel));
				builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Right: ").Append(StringifyExpression(e.Right, indentLevel));
				builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Operation: ").Append(e.Operation);
				break;
			case CallExpression e:
				builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Expression: ").Append(StringifyExpression(e.Expression, indentLevel));

				if (e.Arguments.Count != 0)
				{
					builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).AppendLine("Arguments: [");

					indentLevel++;

					for (var i = 0; i < e.Arguments.Count; i++)
					{
						if (i != 0)
							builder.AppendLine(",");

						builder.Append(new string(' ', indentLevel * 4)).Append(StringifyExpression(e.Arguments[i], indentLevel));
					}

					indentLevel--;

					builder.AppendLine().Append(new string(' ', indentLevel * 4)).Append(']');
				}
				else
					builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Arguments: []");

				break;
			case NumericLiteralExpression e:
				builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Value: ").Append(e.SourceFileFragment.Text);
				break;
			case ReferenceExpression e:
				builder.AppendLine(",").Append(new string(' ', indentLevel * 4)).Append("Name: \"").Append(e.Name).Append('"');
				break;
		}

		indentLevel--;

		return builder.AppendLine().Append(new string(' ', indentLevel * 4)).Append('}').ToString();
	}
}
