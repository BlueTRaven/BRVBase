using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public class NodeGraphAsset : IAsset
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
			public readonly string graphA;
			public readonly string nameA;
			public readonly string graphB;
			public readonly string nameB;
			public readonly string type;
			public readonly float weight;
			public readonly bool valid;

			public Connection(string aGraph, string nameA, string bGraph, string nameB, string type = "default", float weight = 1)
			{
				this.graphA = aGraph;
				this.nameA = nameA;
				this.graphB = bGraph;
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
		private Dictionary<(int, string, int), Connection> connectionsGraph = new Dictionary<(int, string, int), Connection>();

		//Names of other node graphs that this graph depends on.
		private HashSet<string> dependencies = new HashSet<string>();

		private bool finalized;
		private bool connectionsMapped; //connections must be mapped in order to use this node graph.

		public NodeGraphAsset(string name)
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

			if (connection.graphB != name && !dependencies.Contains(connection.graphB))
				dependencies.Add(connection.graphB);

			//TODO: infinite loop (dependency loop)
			//v must be the id in bGraph
			//However, to figure out its id, we must first load that graph and add it as a node so we can map name to node.
			/*int u = nodeNameToIndex[connection.nameA];
			int v = nodeNameToIndex[connection.nameB];

			connectionsGraph.Add((u, connection.bGraph, v), connection);*/
		}

		public void MapConnections(AssetLoaderNodeGraph loader)
		{
			if (finalized)
				throw new Exception("Cannot add a connection after finalization! Write your connection into the node graph file instead!");

			foreach (Connection connection in connections)
			{
				int u = nodeNameToIndex[connection.nameA];
				int v = -1;
				if (connection.graphB == name)
					v = nodeNameToIndex[connection.nameB];
				else v = loader.GetHandle(connection.graphB).Get().nodeNameToIndex[connection.nameB];

				connectionsGraph.Add((u, connection.graphB, v), connection);
			}
		}

		public IReadOnlyList<Node> GetNodes()
		{
			if (finalized && !connectionsMapped)
				throw new Exception("Connections have not been mapped! This is a parsing error!");

			return nodes;
		}

		public IReadOnlyList<Connection> GetConnections()
		{
			if (finalized && !connectionsMapped)
				throw new Exception("Connections have not been mapped! This is a parsing error!");

			return connections;
		}

		public IReadOnlySet<string> GetDependencies()
		{
			if (finalized && !connectionsMapped)
				throw new Exception("Connections have not been mapped! This is a parsing error!");

			return dependencies;
		}

		public int GetIndex(string name)
		{
			if (finalized && !connectionsMapped)
				throw new Exception("Connections have not been mapped! This is a parsing error!");

			if (nodeNameToIndex.ContainsKey(name))
				return nodeNameToIndex[name];
			else return -1;
		}

		//Gets the connection from a to b, if it exists. Otherwise, returns an invalid connection.
		public Connection GetConnection(int a, string bGraph, int b)
		{
			if (finalized && !connectionsMapped)
				throw new Exception("Connections have not been mapped! This is a parsing error!");

			if (connectionsGraph.ContainsKey((a, bGraph, b)))
				return connectionsGraph[(a, bGraph, b)];
			else return default;
		}

		public string GetName()
		{
			return name;
		}
	}

	public class AssetLoaderNodeGraph : AssetLoader<NodeGraphAsset>
	{
		public AssetLoaderNodeGraph() : base(new string[] { Constants.ASSETS_BASE_DIR + "NodeSets/" }, ".pfns")
		{
		}

		private bool loadingDependency;

		protected override NodeGraphAsset Load(LoadableFile file)
		{
			if (File.Exists(file.FullPath))
			{
				Util.WaitForFile(file.FullPath);

				NodeGraphAsset nodeSet = new NodeGraphAsset(file.Name);

				string raw = File.ReadAllText(file.FullPath);

				var tokens = Parser.GetTokens(raw);

				string currentPrefix = "";
				int currentIndex = 0;
				Parser.Token currentToken = tokens[0];

				while (currentToken.type != Parser.Token.TokenType.EndOfFile)
				{
					if (currentToken.type == Parser.Token.TokenType.String)
					{
						if (currentToken.contents == "prefix")
						{
							currentPrefix = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);

							Parser.ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
						else if (currentToken.contents == "nodeslist")
						{
							string start = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string end = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);

							int starti = -1;
							int endi = -1;

							int.TryParse(start, out starti);
							int.TryParse(end, out endi);

							for (int i = starti; i <= endi; i++)
							{
								nodeSet.AddNode(currentPrefix + i.ToString());
							}

							Parser.ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
						else if (currentToken.contents == "connection")
						{
							//Connection format options:
							//connection <a> <optional: graph <b graph>> <b> <optional: type <type>> || <optional: weight <weight>> || <optional: symmetrical>
							//b graph: the graph of point b. This can be something other than the current graph. By default, this is the current graph.
							//	NOTE: does NOT support symmetrical.
							//	NOTE: all graph nodes must be fully specified, and do not support <prefix>.
							string a = currentPrefix + Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string bStr = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string b = currentPrefix;
							string bGraph = file.Name;
							bool bGraphSet = false;

							if (bStr == "graph")
							{
								bGraph = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
								b = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);

								bGraphSet = true;
							}
							else
								b += bStr;

							string type = "default";
							int weight = 1;
							bool isSymmetrical = false;

							bool eol = false;

							while (!eol)
							{
								string content = Parser.MaybeExpectString(ref currentIndex, ref currentToken, tokens, out eol, out _);

								if (eol)
									break;

								if (content == "type")
									type = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
								else if (content == "weight")
								{
									if (!int.TryParse(Parser.ExpectString(ref currentIndex, ref currentToken, tokens), out weight))
										weight = -1;
								}
								else if (content == "symmetrical")
								{
									if (!bGraphSet)
										isSymmetrical = true;
									else throw new Exception("Unexpected string symmetrical! A connection cannot be symmetrical if a B node is defined. After: " + currentToken.contents + ".");
								}
							}

							NodeGraphAsset.Connection connection = new NodeGraphAsset.Connection(file.Name, a, bGraph, b, type, weight);
							nodeSet.AddConnection(connection);

							if (isSymmetrical)
							{
								connection = new NodeGraphAsset.Connection(file.Name, b, bGraph, a, type, weight);
								nodeSet.AddConnection(connection);
							}

							Parser.ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
						else if (currentToken.contents == "sequence")
						{
							string first = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string last = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);

							int.TryParse(first, out int firsti);
							int.TryParse(last, out int lasti);

							for (int i = firsti; i < lasti; i++)
							{
								string a = currentPrefix + i.ToString();
								string b = currentPrefix + (i + 1).ToString();
								NodeGraphAsset.Connection connection = new NodeGraphAsset.Connection(file.Name, a, file.Name, b, "default", 1);
								nodeSet.AddConnection(connection);
							}

							bool eol = false;

							while (!eol)
							{
								string content = Parser.MaybeExpectString(ref currentIndex, ref currentToken, tokens, out eol, out _);

								if (eol)
									break;

								if (content == "loop")
								{
									string a = currentPrefix + lasti.ToString();
									string b = currentPrefix + firsti.ToString();
									NodeGraphAsset.Connection connection = new NodeGraphAsset.Connection(file.Name, a, file.Name, b, "default", 1);
								}
								else if (content == "symmetrical")
								{
									for (int i = lasti; i > firsti; i--)
									{
										string a = currentPrefix + i.ToString();
										string b = currentPrefix + (i - 1).ToString();
										NodeGraphAsset.Connection connection = new NodeGraphAsset.Connection(file.Name, a, file.Name, b, "default", 1);
										nodeSet.AddConnection(connection);
									}
								}
							}
						}
						else if (currentToken.contents == "transition")
						{
							//TODO deprecated?
							string node = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string set = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string transitionNode = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);

							NodeGraphAsset.Connection connection = new NodeGraphAsset.Connection(file.Name, node, file.Name, transitionNode, "transition", 1);
						}
					}

					Parser.NextToken(ref currentIndex, ref currentToken, tokens);
				}

				return nodeSet;
			}

			return default;
		}

		protected override void PostLoad(NodeGraphAsset asset)
		{
			base.PostLoad(asset);

			//We can only map connections once all dependencies have had their nodes resolved.
			//So load all other node sets, which in turn loads all their nodes,
			//then we can map connections for each of them.
			if (!loadingDependency)
			{
				loadingDependency = true;
				//First, load all nodes
				foreach (string dependency in asset.GetDependencies())
				{
					
					GetRaw(dependency);
				}

				//node ids have resolved, so we can now map our connections.
				asset.MapConnections(this);

				//and we finish loading each dependency as well.
				foreach (string dependency in asset.GetDependencies())
				{
					GetRaw(dependency).MapConnections(this);
				}
				loadingDependency = false;
			}
		}

	}
}
