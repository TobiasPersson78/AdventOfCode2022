using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

bool isReadingStartPosition = true;

bool isStartPositionLine(string line) => line.TrimStart().First() == '[';
bool isColumnIndexLine(string line) => Regex.IsMatch(line, @"^(\s+\d+)+\s*$");

(int Count, int From, int To) ParseMoveLine(string line)
{
	IList<int> moveNumbers = Regex
		.Match(line, @"^move (\d+) from (\d+) to (\d+)\s*$")
		.Groups
		.Values
		.Skip(1)
		.Select(group => int.Parse(group.Value))
		.ToList();
	return (moveNumbers[0], moveNumbers[1], moveNumbers[2]);
}

IList<(int Column, IList<char> Items)> startPositionContent = new List<(int Column, IList<char> Items)>
{
	(1, new List<char>()),
	(5, new List<char>()),
	(9, new List<char>()),
	(13, new List<char>()),
	(17, new List<char>()),
	(21, new List<char>()),
	(25, new List<char>()),
	(29, new List<char>()),
	(33, new List<char>()),
};

IList<Stack<char>>? stacksPartA = null;
IList<Stack<char>>? stacksPartB = null;

foreach (string row in File.ReadAllLines(inputFilename).Where(item => !string.IsNullOrEmpty(item)))
{
	if (isReadingStartPosition)
	{
		if (isColumnIndexLine(row))
		{
			isReadingStartPosition = false;

			stacksPartA = startPositionContent
				.Select(columnAndItems => new Stack<char>(columnAndItems.Items.Reverse()))
				.ToList();
			stacksPartB = startPositionContent
				.Select(columnAndItems => new Stack<char>(columnAndItems.Items.Reverse()))
				.ToList();
			continue;
		}

		foreach ((int column, IList<char> items) in startPositionContent)
		{
			if (row.Length > column && !char.IsWhiteSpace(row[column]))
				items.Add(row[column]);
		}
	}
	else
	{
		var (count, from, to) = ParseMoveLine(row);

		List<char> partBTemporaryContainer = new();
		while (count-- > 0)
		{
			stacksPartA[to - 1].Push(stacksPartA[from - 1].Pop());
			partBTemporaryContainer.Add(stacksPartB[from - 1].Pop());
		}

		partBTemporaryContainer.Reverse();
		foreach (char currentItem in partBTemporaryContainer)
		{
			stacksPartB[to - 1].Push(currentItem);
		}
	}
}

string topOfStacksPartA = string.Concat(stacksPartA.Where(stack => stack.Any()).Select(stack => stack.Peek()));
string topOfStacksPartB = string.Concat(stacksPartB.Where(stack => stack.Any()).Select(stack => stack.Peek()));

Console.WriteLine("Day 5A");
Console.WriteLine($"Top of the stacks: {topOfStacksPartA}");

Console.WriteLine("Day 5B");
Console.WriteLine($"Top of the stacks: {topOfStacksPartB}");