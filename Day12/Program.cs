bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

// Get height map, start position and end position as "global" variables.
IList<IList<char>> heightMap =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.Select(item => (IList<char>)item.ToCharArray())
		.ToList();

int numberOfColumns = heightMap.First().Count;
int numberOfRows = heightMap.Count;

int startPosition = -1;
int endPosition = -1;

for (int y = 0; y < numberOfRows && (startPosition < 0 || endPosition < 0); ++y)
{
	if (startPosition < 0)
	{
		int positionOfS = heightMap[y].IndexOf('S');
		if (positionOfS >= 0)
		{
			startPosition = GetIdFromPosition(positionOfS, y);
			heightMap[y][positionOfS] = 'a';
		}
	}
	if (endPosition < 0)
	{
		int positionOfE = heightMap[y].IndexOf('E');
		if (positionOfE >= 0)
		{
			endPosition = GetIdFromPosition(positionOfE, y);
			heightMap[y][positionOfE] = 'z';
		}
	}
}

var numberOfStepsPartA = SearchNodesPartA();
var numberOfStepsPartB = SearchNodesPartB();
Console.WriteLine("Day 12A");
Console.WriteLine($"Minimum number of steps from start up to end: {numberOfStepsPartA}");

Console.WriteLine("Day 12B");
Console.WriteLine($"Minimum number of steps from end down to height 'a': {numberOfStepsPartB}");

int SearchNodesPartA()
{
	var allNodes = GetNodes(GetEdgesForPositionPartA);
	Node endNode = allNodes[endPosition];
	DijkstraSearch(
		allNodes,
		allNodes[startPosition],
		node => node == endNode);
	var fromStartToEnd = GetPathToEnd(endNode);
	return fromStartToEnd.Count - 1;
}

int SearchNodesPartB()
{
	var allNodes = GetNodes(GetEdgesForPositionPartB);
	// Go from the end node to the first available node of height 'a'.
	Node startNode = allNodes[endPosition];
	Node? firstANode = DijkstraSearch(
		allNodes,
		startNode,
		node => heightMap[node.Position.Y][node.Position.X] == 'a');
	if (firstANode == null)
		throw new InvalidOperationException("Found no 'a' node in reverse.");

	var fromEndToFirstANode = GetPathToEnd(firstANode);
	return fromEndToFirstANode.Count - 1;
}

IList<Node> GetNodes(Func<IList<Node>, int, int, IEnumerable<(Node Node, int Cost)>> edgeGenerator)
{
	int lastId = -1;
	IList<Node> allNodes = new List<Node>();

	for (int y = 0; y < numberOfRows; ++y)
	{
		for (int x = 0; x < numberOfColumns; ++x)
		{
			Node newNode = new() { Id = ++lastId, Position = (x, y) };
			allNodes.Add(newNode);
		}
	}

	for (int y = 0; y < numberOfRows; ++y)
	{
		for (int x = 0; x < numberOfColumns; ++x)
		{
			int id = GetIdFromPosition(x, y);

			foreach (var edgeForNode in edgeGenerator(allNodes, x, y))
			{
				allNodes[id].Edges.Add(edgeForNode);
			}
		}
	}

	return allNodes;
}

Node? DijkstraSearch(IList<Node> nodes, Node startNode, Func<Node, bool> endCondition)
{
	startNode.MinimumCostToStart = 0;

	var priorityQueue = new List<Node> { startNode };

	do
	{
		priorityQueue = priorityQueue.OrderBy(node => node.MinimumCostToStart).ToList();
		var currentNode = priorityQueue.First();
		priorityQueue.RemoveAt(0);

		foreach (var currentEdge in currentNode.Edges.Where(edge => !edge.Node.Visited))
		{
			var childNode = currentEdge.Node;

			if (childNode.MinimumCostToStart == int.MaxValue ||
				childNode.MinimumCostToStart + currentEdge.Cost < childNode.MinimumCostToStart)
			{
				childNode.MinimumCostToStart = currentNode.MinimumCostToStart + currentEdge.Cost;
				childNode.NearestToStart = currentNode;

				if (!priorityQueue.Contains(childNode))
				{
					priorityQueue.Add(childNode);
				}
			}
		}

		currentNode.Visited = true;

		if (endCondition(currentNode))
			return currentNode;

	} while (priorityQueue.Any());

	return null;
}

IList<Node> GetPathToEnd(Node endNode1)
{
	IList<Node> pathToEnd;
	{
		IList<Node> pathToStart = new List<Node>();
		Node? currentNode = endNode1;
		while (currentNode != null)
		{
			pathToStart.Add(currentNode);
			currentNode = currentNode.NearestToStart;
		}

		pathToEnd = ((IEnumerable<Node>)pathToStart).Reverse().ToList();
	}

	return pathToEnd;
}

int GetIdFromPosition(int x, int y) => y * numberOfColumns + x;

IEnumerable<(Node Node, int Cost)> GetEdgesForPositionPartA(IList<Node> allNodes, int x, int y)
{
	char maxHeight = heightMap[y][x];
	maxHeight++;

	const int DefaultCost = 1;

	// Up
	if (y > 0 && heightMap[y - 1][x] <= maxHeight)
		yield return (allNodes[GetIdFromPosition(x, y - 1)], DefaultCost);

	// Down
	if (y < numberOfRows - 1 && heightMap[y + 1][x] <= maxHeight)
		yield return (allNodes[GetIdFromPosition(x, y + 1)], DefaultCost);

	// Left
	if (x > 0 && heightMap[y][x - 1] <= maxHeight)
		yield return (allNodes[GetIdFromPosition(x - 1, y)], DefaultCost);

	// Right
	if (x < numberOfColumns - 1 && heightMap[y][x + 1] <= maxHeight)
		yield return (allNodes[GetIdFromPosition(x + 1, y)], DefaultCost);
}

IEnumerable<(Node Node, int Cost)> GetEdgesForPositionPartB(IList<Node> allNodes, int x, int y)
{
	char minHeight = heightMap[y][x];
	minHeight--;

	const int DefaultCost = 1;

	// Up
	if (y > 0 && heightMap[y - 1][x] >= minHeight)
		yield return (allNodes[GetIdFromPosition(x, y - 1)], DefaultCost);

	// Down
	if (y < numberOfRows - 1 && heightMap[y + 1][x] >= minHeight)
		yield return (allNodes[GetIdFromPosition(x, y + 1)], DefaultCost);

	// Left
	if (x > 0 && heightMap[y][x - 1] >= minHeight)
		yield return (allNodes[GetIdFromPosition(x - 1, y)], DefaultCost);

	// Right
	if (x < numberOfColumns - 1 && heightMap[y][x + 1] >= minHeight)
		yield return (allNodes[GetIdFromPosition(x + 1, y)], DefaultCost);
}

class Node
{
	public int Id { get; init; }

	public (int X, int Y) Position { get; init; }

	public IList<(Node Node, int Cost)> Edges { get; } = new List<(Node Node, int Cost)>();

	public bool Visited { get; set; }

	public int MinimumCostToStart { get; set; } = int.MaxValue;
	public Node? NearestToStart { get; set; }

	public override string ToString() => "Node " + Position.ToString();
}
