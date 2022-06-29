using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public class PathfindNodeSet : IAsset
	{
		public readonly struct Node
		{
			public readonly string name;
			public readonly int id;

			public Node(string name, int id)
			{
				this.name = name;
				this.id = id;
			}
		}

		public readonly struct Connection 
		{
			public readonly string nameA;
			public readonly string nameB;
			public readonly string type;
			public readonly float weight;
			public readonly bool valid;

			public Connection(string nameA, string nameB, string type = "default", float weight = 1)
			{
				this.nameA = nameA;
				this.nameB = nameB;
				this.type = type;
				this.weight = weight;

				valid = true;
			}
		}

		private string name;

		public int NodesCount => nodes.Count;
		public int ConnectionsCount => connections.Count;

		private List<Node> nodes = new List<Node>();
		private Dictionary<string, int> nodeNameToIndex = new Dictionary<string, int>();

		private List<Connection> connections = new List<Connection>();
		private Dictionary<(int, int), Connection> connectionsGraph = new Dictionary<(int, int), Connection>();

		private bool finalized;
		private float[,] dist;
		private int[,] next;

		public PathfindNodeSet(string name)
		{
			this.name = name;
		}

		public void AddNode(string name)
		{
			if (finalized)
				throw new Exception("Cannot add a node after finalization! Write your node into the node graph file instead!");

			int index = nodes.Count;
			nodes.Add(new Node(name, index));
			nodeNameToIndex.Add(name, index);
		}

		public void AddConnection(Connection connection)
		{
			if (finalized)
				throw new Exception("Cannot add a connection after finalization! Write your connection into the node graph file instead!");

			connections.Add(connection);

			int u = nodeNameToIndex[connection.nameA];
			int v = nodeNameToIndex[connection.nameB];

			connectionsGraph.Add((u, v), connection);
		}

		public List<Node> GetNodes()
		{
			return nodes;
		}

		public List<Connection> GetConnections()
		{
			return connections;
		}

		public int GetIndex(string name)
		{
			return nodeNameToIndex[name];
		}

		//Gets the connection from a to b, if it exists. Otherwise, returns an invalid connection.
		public Connection GetConnection(int a, int b)
		{
			if (connectionsGraph.ContainsKey((a, b)))
				return connectionsGraph[(a, b)];
			else return default;
		}

		public string GetName()
		{
			return name;
		}
	}

	public class AssetLoaderPathfindNodeSet : AssetLoader<PathfindNodeSet>
	{
		public AssetLoaderPathfindNodeSet() : base(new string[] { "./Assets/NodeSets/" }, ".pfns")
		{
		}

		protected override PathfindNodeSet Load(LoadableFile file)
		{
			if (File.Exists(file.FullPath))
			{
				Util.WaitForFile(file.FullPath);

				PathfindNodeSet nodeSet = new PathfindNodeSet(file.Name);

				string raw = File.ReadAllText(file.FullPath);

				var tokens = GetTokens(raw);

				string currentPrefix = "";
				int currentIndex = 0;
				Token currentToken = tokens[0];

				while (currentToken.type != Token.TokenType.EndOfFile)
				{
					if (currentToken.type == Token.TokenType.String)
					{
						if (currentToken.contents == "prefix")
						{
							currentPrefix = ExpectString(ref currentIndex, ref currentToken, tokens);

							ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
						else if (currentToken.contents == "nodeslist")
						{
							string start = ExpectString(ref currentIndex, ref currentToken, tokens);
							string end = ExpectString(ref currentIndex, ref currentToken, tokens);

							int starti = -1;
							int endi = -1;

							int.TryParse(start, out starti);
							int.TryParse(end, out endi);

							for (int i = starti; i <= endi; i++)
							{
								nodeSet.AddNode(currentPrefix + i.ToString());
							}

							ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
						else if (currentToken.contents == "connection")
						{
							string a = currentPrefix + ExpectString(ref currentIndex, ref currentToken, tokens);
							string b = currentPrefix + ExpectString(ref currentIndex, ref currentToken, tokens);
							string type = "default";
							int weight = 1;
							bool isSymmetrical = false;

							bool eol = false;

							while (!eol)
							{
								string content = MaybeExpectString(ref currentIndex, ref currentToken, tokens, out eol);

								if (eol)
									break;

								if (content == "type")
									type = ExpectString(ref currentIndex, ref currentToken, tokens);
								else if (content == "weight")
								{
									if (!int.TryParse(ExpectString(ref currentIndex, ref currentToken, tokens), out weight))
										weight = -1;
								}
								else if (content == "symmetrical")
									isSymmetrical = true;
							}

							PathfindNodeSet.Connection connection = new PathfindNodeSet.Connection(a, b, type, weight);
							nodeSet.AddConnection(connection);

							if (isSymmetrical)
							{
								connection = new PathfindNodeSet.Connection(b, a, type, weight);
								nodeSet.AddConnection(connection);
							}

							ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
						else if (currentToken.contents == "sequence")
						{
							string first = ExpectString(ref currentIndex, ref currentToken, tokens);
							string last = ExpectString(ref currentIndex, ref currentToken, tokens);

							int.TryParse(first, out int firsti);
							int.TryParse(last, out int lasti);

							for (int i = firsti; i < lasti; i++)
							{
								string a = currentPrefix + i.ToString();
								string b = currentPrefix + (i + 1).ToString();
								PathfindNodeSet.Connection connection = new PathfindNodeSet.Connection(a, b, "default", 1);
								nodeSet.AddConnection(connection);
							}

							bool eol = false;

							while (!eol)
							{
								string content = MaybeExpectString(ref currentIndex, ref currentToken, tokens, out eol);

								if (eol)
									break;

								if (content == "loop")
								{
									string a = currentPrefix + lasti.ToString();
									string b = currentPrefix + firsti.ToString();
									PathfindNodeSet.Connection connection = new PathfindNodeSet.Connection(a, b, "default", 1);
								}
								else if (content == "symmetrical")
								{
									for (int i = lasti; i > firsti; i--)
									{
										string a = currentPrefix + i.ToString();
										string b = currentPrefix + (i - 1).ToString();
										PathfindNodeSet.Connection connection = new PathfindNodeSet.Connection(a, b, "default", 1);
										nodeSet.AddConnection(connection);
									}
								}
							}
						}
						else if (currentToken.contents == "transition")
						{
							string node = ExpectString(ref currentIndex, ref currentToken, tokens);
							string set = ExpectString(ref currentIndex, ref currentToken, tokens);
							string transitionNode = ExpectString(ref currentIndex, ref currentToken, tokens);

							PathfindNodeSet.Connection connection = new PathfindNodeSet.Connection(node, transitionNode, "transition", 1);
						}
					}

					NextToken(ref currentIndex, ref currentToken, tokens);
				}

				return nodeSet;
			}

			return default;
		}

		private void NextToken(ref int index, ref Token currentToken, Token[] tokens)
		{
			index++;
			currentToken = tokens[index];
		}

		private void ExpectEndOfLine(ref int index, ref Token currentToken, Token[] tokens)
		{
			if (currentToken.type == Token.TokenType.EndOfLine)
				return;

			string startTokenContent = currentToken.contents;

			NextToken(ref index, ref currentToken, tokens);

			if (currentToken.type != Token.TokenType.EndOfLine)
				throw new Exception("Unexpected token! Expecting an end of line (';') after " + startTokenContent + ".");
		}

		private string ExpectString(ref int index, ref Token currentToken, Token[] tokens)
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
		private string MaybeExpectString(ref int index, ref Token currentToken, Token[] tokens, out bool reachedEndOfLine)
		{
			string startTokenContent = currentToken.contents;

			NextToken(ref index, ref currentToken, tokens);

			while (currentToken.type != Token.TokenType.String)
			{
				if (currentToken.type == Token.TokenType.EndOfLine)
				{
					reachedEndOfLine = true;
					return currentToken.contents;
				}
				else if (currentToken.type == Token.TokenType.EndOfFile)
					throw new Exception("Unexpected end of line! Expecting a string after " + startTokenContent + ".");

				NextToken(ref index, ref currentToken, tokens);
			}

			reachedEndOfLine = false;
			return currentToken.contents;
		}

		private struct Token
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
		
		private Token[] GetTokens(string str)
		{
			int i = 0;
			int row = 0;	//increment after a \n
			int col = 0;

			List<Token> tokens = new List<Token>();
			Token.TokenType currentType = Token.TokenType.String;
			StringBuilder currentContent = new StringBuilder();

			if (str[i] == ' ')
				currentType = Token.TokenType.Space;
			else if (str[i] == ';')
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
					else if (str[i] == ';')
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
