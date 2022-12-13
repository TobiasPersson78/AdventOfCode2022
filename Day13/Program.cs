bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

IList<Node> allNodes =
    File
        .ReadAllLines(inputFilename)
        .Where(item => !string.IsNullOrEmpty(item))
        .Select(item => ParseRow(item))
        .ToList();

int sumOfIndicesOfCorrectOrderPairs = 0;

for (int pairIndex = 1; pairIndex <= allNodes.Count / 2; pairIndex++)
{
	Node left = allNodes[2 * pairIndex - 2];
	Node right = allNodes[2 * pairIndex - 1];

	Console.Write($"Pair {pairIndex}: Comparing {left} and {right}: ");

	int result = CompareNodes(left, right);
	if (result < 0)
	{
		Console.WriteLine("Order is correct.");
		sumOfIndicesOfCorrectOrderPairs += pairIndex;
	}
	else if (result > 0)
	{
		Console.WriteLine("Order is wrong.");
	}
	else
	{
		Console.WriteLine("Nodes are equal.");
	}
}

Node dividerPacketTwo = ParseRow("[[2]]");
Node dividerPacketSix = ParseRow("[[6]]");
List<Node> allNodesWithDividers = allNodes.Concat(new[] { dividerPacketTwo, dividerPacketSix }).ToList();
allNodesWithDividers.Sort(CompareNodes);
int dividerPacketTwoIndex = allNodesWithDividers.IndexOf(dividerPacketTwo);
int dividerPacketSixIndex = allNodesWithDividers.IndexOf(dividerPacketSix);
int decoderKey = (dividerPacketTwoIndex + 1) * (dividerPacketSixIndex + 1);

Console.WriteLine("Day 13A");
Console.WriteLine($"Sum of indices of pairs in correct order: {sumOfIndicesOfCorrectOrderPairs}");

Console.WriteLine("Day 13B");
Console.WriteLine($"Decoder key: {decoderKey}");

Node ParseRow(string row)
{
    if (row[0] != '[')
        throw new IOException($"Unexpected start of row: {row}.");

    (Node ParsedNode, int EndPosition) = ParseUntilEnd(row, 1);

    return ParsedNode;
}

(Node ParsedNode, int EndPosition) ParseUntilEnd(string row, int startPosition)
{
    IList<Node> childNodes = new List<Node>();
    string currentNumber = string.Empty;

    for (int index = startPosition; ; ++index)
    {
        switch (row[index])
        {
            case '[':
                (Node childNode, int endPosition) = ParseUntilEnd(row, index + 1);
                childNodes.Add(childNode);
                index = endPosition;
                break;
            case ']':
                if (childNodes.Any())
                {
                    if (currentNumber.Any())
                    {
                        int intValue = int.Parse(currentNumber);
                        childNodes.Add(new Node(intValue));
                    }
                    return (new Node(childNodes), index);
                }
                if (currentNumber.Any())
                {
                    int intValue = int.Parse(currentNumber);
                    return (new Node(intValue), index);
                }
                return (new Node(), index);
            case ',':
                {
                    if (currentNumber.Any())
                    {
                        int intValue = int.Parse(currentNumber);
                        childNodes.Add(new Node(intValue));
                        currentNumber = string.Empty;
                    }
                }
                break;
            default:
                currentNumber += row[index];
                break;
        }
    }
}

int CompareNodes(Node left, Node right)
{
    // Both nodes are integers.
    if (left.IsIntValue && right.IsIntValue)
        return left.Value - right.Value;

    // Both nodes are lists.
    if (!left.IsIntValue && !right.IsIntValue)
    {
        int minChildren = Math.Min(left.ChildNodes.Count, right.ChildNodes.Count);
        for (int index = 0; index < minChildren; ++index)
        {
            int result = CompareNodes(left.ChildNodes[index], right.ChildNodes[index]);
            if (result != 0)
                return result;
        }

        return left.ChildNodes.Count - right.ChildNodes.Count;
    }

    // One of the nodes is an integer, the other a list.
    Node leftAsList = left.IsIntValue
        ? new Node(new[] { left })
        : left;
    Node rightAsList = right.IsIntValue
        ? new Node(new[] { right })
        : right;
    return CompareNodes(leftAsList, rightAsList);
}

class Node
{
    int intValue;
    IList<Node> childNodes = new Node[] { };

    public bool IsIntValue { get; }
    
	public int Value
	{
		get
		{
			if (!IsIntValue)
				throw new InvalidOperationException("Node does not contain an integer.");

			return intValue;
		}
	}

    public IList<Node> ChildNodes
	{
		get
		{
			if (IsIntValue)
				throw new InvalidOperationException("Node does not contain child nodes.");

			return childNodes;
		}
	}

    public Node(int value)
	{
		IsIntValue = true;
		intValue = value;
	}

    public Node(IEnumerable<Node> childNodes)
    {
        IsIntValue = false;
		this.childNodes = childNodes.ToList().AsReadOnly();
    }

    public Node()
		: this(Enumerable.Empty<Node>())
    {
    }

    public override string ToString()
    {
		if (IsIntValue)
			return intValue.ToString();

		return '[' + string.Join(',', childNodes.Select(node => node.ToString())) + ']';
    }
}