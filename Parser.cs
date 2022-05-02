using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public static class Parser
	{
		public static void NextToken(ref int index, ref Token currentToken, Token[] tokens)
		{
			index++;
			currentToken = tokens[index];
		}

		public static void ExpectEndOfLine(ref int index, ref Token currentToken, Token[] tokens)
		{
			if (currentToken.type == Token.TokenType.EndOfLine)
				return;

			string startTokenContent = currentToken.contents;

			NextToken(ref index, ref currentToken, tokens);

			if (currentToken.type != Token.TokenType.EndOfLine)
				throw new Exception("Unexpected token! Expecting an end of line (';') after " + startTokenContent + ".");
		}

		public static string ExpectString(ref int index, ref Token currentToken, Token[] tokens)
		{
			string startTokenContent = currentToken.contents;

			NextToken(ref index, ref currentToken, tokens);

			while (currentToken.type != Token.TokenType.String)
			{
				if (currentToken.type == Token.TokenType.EndOfLine)
					throw new Exception("Unexpected end of line! Expecting a string after " + startTokenContent + ".");
				else if (currentToken.type == Token.TokenType.EndOfFile)
					throw new Exception("Unexpected end of line! Expecting a string after " + startTokenContent + ".");

				NextToken(ref index, ref currentToken, tokens);
			}

			return currentToken.contents;
		}

		//Same as ExpectString, but can take end of line.
		public static string MaybeExpectString(ref int index, ref Token currentToken, Token[] tokens, out bool reachedEndOfLine, out bool reachedEndOfFile)
		{
			string startTokenContent = currentToken.contents;

			NextToken(ref index, ref currentToken, tokens);

			while (currentToken.type != Token.TokenType.String)
			{
				if (currentToken.type == Token.TokenType.EndOfLine)
				{
					reachedEndOfLine = true;
					reachedEndOfFile = false;
					return currentToken.contents;
				}
				else if (currentToken.type == Token.TokenType.EndOfFile)
				{
					reachedEndOfFile = true;
					reachedEndOfLine = true;
					return currentToken.contents;
				}

				NextToken(ref index, ref currentToken, tokens);
			}

			reachedEndOfLine = false;
			reachedEndOfFile = false;
			return currentToken.contents;
		}

		public struct Token
		{
			public enum TokenType
			{
				StartOfFile,
				EndOfLine,
				EndOfFile,
				String,
				Space
			}

			public TokenType type;
			public string contents;

			public Token(TokenType type, string contents)
			{
				this.type = type;
				this.contents = contents;
			}

			public override string ToString()
			{
				return "Token " + type.ToString() + ": " + contents;
			}
		}

		public static Token[] GetTokens(string str)
		{
			int i = 0;
			int row = 0;    //increment after a \n
			int col = 0;

			List<Token> tokens = new List<Token>();
			Token.TokenType currentType = Token.TokenType.String;
			StringBuilder currentContent = new StringBuilder();

			if (str[i] == ' ')
				currentType = Token.TokenType.Space;
			else if (str[i] == ';' || str[i] == '\n')
				currentType = Token.TokenType.EndOfLine;
			else currentType = Token.TokenType.String;

			tokens.Add(new Token(Token.TokenType.StartOfFile, ""));

			while (i < str.Length)
			{
				if (currentType == Token.TokenType.EndOfLine)
				{
					tokens.Add(new Token(currentType, currentContent.ToString()));
					currentContent.Clear();
					row++;

					if (str[i] == ' ')
						currentType = Token.TokenType.Space;
					else if (str[i] == ';' || str[i] == '\n')
						currentType = Token.TokenType.EndOfLine;
					else currentType = Token.TokenType.String;
				}
				else if (currentType == Token.TokenType.String)
				{
					if (str[i] == ' ')
					{
						tokens.Add(new Token(currentType, currentContent.ToString().Trim()));
						currentContent.Clear();
						currentType = Token.TokenType.Space;
					}
					else if (str[i] == ';')
					{
						tokens.Add(new Token(currentType, currentContent.ToString().Trim()));
						currentContent.Clear();
						currentType = Token.TokenType.EndOfLine;
					}
					else if (str[i] == '\n')
					{
						tokens.Add(new Token(currentType, currentContent.ToString().Trim()));
						currentContent.Clear();
						currentType = Token.TokenType.EndOfLine;
					}
				}
				else if (currentType == Token.TokenType.Space)
				{
					if (str[i] != ' ')
					{
						if (str[i] == ';')
						{
							tokens.Add(new Token(currentType, currentContent.ToString()));
							currentContent.Clear();
							currentType = Token.TokenType.EndOfLine;
						}
						else
						{
							tokens.Add(new Token(currentType, currentContent.ToString()));
							currentContent.Clear();
							currentType = Token.TokenType.String;
						}
					}
				}

				currentContent.Append(str[i]);
				col = i;
				i++;
			}

			tokens.Add(new Token(currentType, currentContent.ToString()));
			tokens.Add(new Token(Token.TokenType.EndOfFile, currentContent.ToString()));

			return tokens.ToArray();
		}
	}
}
