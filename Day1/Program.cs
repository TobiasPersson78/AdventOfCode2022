bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

IList<int> caloriesForAllElves = new List<int>();
int currentElfCalories = 0;

foreach (string currentRow in File.ReadAllLines(inputFilename))
{
	if (string.IsNullOrEmpty(currentRow))
	{
		caloriesForAllElves.Add(currentElfCalories);
		currentElfCalories = 0;
		continue;
	}

	currentElfCalories += int.Parse(currentRow);
}

if (currentElfCalories > 0)
	caloriesForAllElves.Add(currentElfCalories);

Console.WriteLine("Day 1a");
int maxCalories = caloriesForAllElves.Max();
Console.WriteLine($"The maximum number of calories is {maxCalories}.");

Console.WriteLine("Day 1b");
int sumTopThreeCalories = caloriesForAllElves.OrderDescending().Take(3).Sum();
Console.WriteLine($"The sum of calories for the three highest values is {sumTopThreeCalories}.");
