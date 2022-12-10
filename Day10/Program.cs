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
const int rowWidth = 40;
const char litChar = '#';
const char darkChar = '.';
const int spriteWidth = 3;
StringBuilder currentLine = new();

foreach (int xChange in GetXChanges())
{
	++cycleCounter;
	if (cyclesToCheckSignalStrengths.Contains(cycleCounter))
		sumOfSignalStrengths += xRegister * cycleCounter;

	int currentColumn = (cycleCounter - 1) % rowWidth;

	char currentChar = Math.Abs(xRegister - currentColumn) <= spriteWidth / 2
		? litChar
		: darkChar;
	currentLine.Append(currentChar);

	Debug.WriteLine($"During cycle {cycleCounter}, X is {xRegister}, current line is {currentLine}");

	if (currentColumn % rowWidth == rowWidth - 1)
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
