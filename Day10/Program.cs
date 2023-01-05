using System.Diagnostics;
using System.Text;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

IEnumerable<int> GetXChanges()
{
	foreach (string row in File.ReadAllLines(inputFilename))
	{
		yield return 0;

		if (row.StartsWith("addx"))
		{
			yield return int.Parse(row[5..]);
		}
	}
}

ISet<int> cyclesToCheckSignalStrengths = new HashSet<int>
{
	20,
	60,
	100,
	140,
	180,
	220,
};
int sumOfSignalStrengths = 0;
int cycleCounter = 0;
int xRegister = 1;
const int RowWidth = 40;
const char LitChar = '#';
const char DarkChar = '.';
const int SpriteWidth = 3;
StringBuilder currentLine = new();

foreach (int xChange in GetXChanges())
{
	++cycleCounter;
	if (cyclesToCheckSignalStrengths.Contains(cycleCounter))
		sumOfSignalStrengths += xRegister * cycleCounter;

	int currentColumn = (cycleCounter - 1) % RowWidth;

	char currentChar = Math.Abs(xRegister - currentColumn) <= SpriteWidth / 2
		? LitChar
		: DarkChar;
	currentLine.Append(currentChar);

	Debug.WriteLine($"During cycle {cycleCounter}, X is {xRegister}, current line is {currentLine}");

	if (currentColumn % RowWidth == RowWidth - 1)
	{
		Console.WriteLine(currentLine.ToString());
		currentLine.Clear();
	}

	xRegister += xChange;
}

Console.WriteLine("Day 10A");
Console.WriteLine($"Sum of signal strengths: {sumOfSignalStrengths}");

Console.WriteLine("Day 10B");
Console.WriteLine("See CRT text above.");
