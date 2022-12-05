using System.Collections.Immutable;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

int sumOfIndividualPriorities = 0;
int sumOfGroupOfThreeBadgePriorities = 0;
ISet<char> groupOfThreeTypeOfItems = new HashSet<char>();

foreach ((string currentRow, int index)
		 in File
			 .ReadAllLines(inputFilename)
			 .Where(item => !string.IsNullOrEmpty(item))
			 .Select((row, index) => (row, index)))
{
	var firstCompartmentContent = currentRow.Substring(0, currentRow.Length / 2);
	var secondCompartmentContent = currentRow.Substring(currentRow.Length / 2, currentRow.Length / 2);
	IImmutableSet<char> firstCompartmentTypeOfItems = firstCompartmentContent.ToImmutableHashSet();
	IImmutableSet<char> secondCompartmentTypeOfItems = secondCompartmentContent.ToImmutableHashSet();

	var sharedTypesOfItems = firstCompartmentTypeOfItems.Intersect(secondCompartmentTypeOfItems);

	if (sharedTypesOfItems.Count != 1)
		throw new InvalidOperationException("Unexpected number of shared items.");

	char sharedItem = sharedTypesOfItems.First();

	int GetPriorityForItem(char item) =>
		(item >= 'a' && item <= 'z')
			? item - 'a' + 1
			: item - 'A' + 27;

	sumOfIndividualPriorities += GetPriorityForItem(sharedItem);

	if (index % 3 == 0)
	{
		// Start of a new group of three.
		groupOfThreeTypeOfItems = new HashSet<char>(currentRow);
	}
	else
	{
		groupOfThreeTypeOfItems.IntersectWith(currentRow.ToHashSet());
	}

	if (index % 3 == 2)
	{
		if (groupOfThreeTypeOfItems.Count != 1)
			throw new InvalidOperationException("Unexpected number of group badges.");

		char groupOfThreeItem = groupOfThreeTypeOfItems.First();

		sumOfGroupOfThreeBadgePriorities += GetPriorityForItem(groupOfThreeItem);
	}
}

Console.WriteLine("Day 3A");
Console.WriteLine($"Total priority score: {sumOfIndividualPriorities}");

Console.WriteLine("Day 3B");
Console.WriteLine($"Sum of group of three badge priorities: {sumOfGroupOfThreeBadgePriorities}");