using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

int numberOfFullyOverlappingAssignments = 0;
int numberOfPartiallyOverlappingAssignments = 0;

foreach (((int Start, int End) firstElf, (int Start, int End) secondElf)
		in File
			.ReadAllLines(inputFilename)
			.Where(item => !string.IsNullOrEmpty(item))
			.Select(row =>
				Regex
					.Match(row, @"(\d+)-(\d+),(\d+)-(\d+)")
					.Groups
					.Values
					.Skip(1)
					.Select(group => int.Parse(group.Value))
					.ToList())
			 .Select(listOfInts =>
				((listOfInts[0], listOfInts[1]),
					(listOfInts[2], listOfInts[3]))))
{
	if ((firstElf.Start <= secondElf.Start && firstElf.End >= secondElf.End) ||
		(firstElf.Start >= secondElf.Start && firstElf.End <= secondElf.End))
	{
		numberOfFullyOverlappingAssignments++;
	}

	bool IsBetween(int start, int end, int valueToTest) =>
		(start <= valueToTest) && (end >= valueToTest);

	if (IsBetween(firstElf.Start, firstElf.End, secondElf.Start) ||
		IsBetween(firstElf.Start, firstElf.End, secondElf.End) ||
		IsBetween(secondElf.Start, secondElf.End, firstElf.Start) ||
		IsBetween(secondElf.Start, secondElf.End, firstElf.End))
	{
		numberOfPartiallyOverlappingAssignments++;
	}
}

Console.WriteLine("Day 4A");
Console.WriteLine($"Number of fully overlapping assignments: {numberOfFullyOverlappingAssignments}");

Console.WriteLine("Day 4B");
Console.WriteLine($"Number of partially overlapping assignments: {numberOfPartiallyOverlappingAssignments}");
