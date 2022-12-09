bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

List<char[]> treeHeights = File
	.ReadAllLines(inputFilename)
	.Where(item => !string.IsNullOrEmpty(item))
	.Select(row => row.ToCharArray())
	.ToList();

int width = treeHeights[0].Length;
int height = treeHeights.Count;

bool[,] isVisibleMatrix = new bool[height, width];

// Mark borders as visible.
for (int column = 0; column < width; column++)
{
	isVisibleMatrix[0, column] = true;
	isVisibleMatrix[height - 1, column] = true;
}
for (int row = 0; row < height; row++)
{
	isVisibleMatrix[row, 0] = true;
	isVisibleMatrix[row, width - 1] = true;
}

for (int row = 1; row < height - 1; row++)
{
	char currentMax;

	// Check if visible from the left.
	currentMax = treeHeights[row][0];
	for (int column = 1; column < width - 1; column++)
	{
		if (treeHeights[row][column] > currentMax)
		{
			currentMax = treeHeights[row][column];
			isVisibleMatrix[row, column] = true;
		}
	}

	// Check if visible from the right.
	currentMax = treeHeights[row][width - 1];
	for (int column = width - 2; column > 0; column--)
	{
		if (treeHeights[row][column] > currentMax)
		{
			currentMax = treeHeights[row][column];
			isVisibleMatrix[row, column] = true;
		}
	}
}

for (int column = 1; column < width - 1; column++)
{
	char currentMax;

	// Check if visible from the top.
	currentMax = treeHeights[0][column];
	for (int row = 1; row < height - 1; row++)
	{
		if (treeHeights[row][column] > currentMax)
		{
			currentMax = treeHeights[row][column];
			isVisibleMatrix[row, column] = true;
		}
	}

	// Check if visible from the bottom.
	currentMax = treeHeights[height - 1][column];
	for (int row = height - 2; row > 0; row--)
	{
		if (treeHeights[row][column] > currentMax)
		{
			currentMax = treeHeights[row][column];
			isVisibleMatrix[row, column] = true;
		}
	}
}

int numberOfVisibleTrees = isVisibleMatrix
	.ToEnumerable()
	.Count(isVisible => isVisible);

int TreesLessThanHeightUpFrom(int row, int column)
{
	int steps = 1;
	int maxHeight = treeHeights[row][column];
	while (row - steps > 0 && treeHeights[row - steps][column] < maxHeight)
		++steps;
	return steps;
}

int TreesLessThanHeightDownFrom(int row, int column)
{
	int steps = 1;
	int maxHeight = treeHeights[row][column];
	while (row + steps < height - 1 && treeHeights[row + steps][column] < maxHeight)
		++steps;

	return steps;
}

int TreesLessThanHeightLeftFrom(int row, int column)
{
	int steps = 1;
	int maxHeight = treeHeights[row][column];
	while (column - steps > 0 && treeHeights[row][column - steps] < maxHeight)
		++steps;

	return steps;
}

int TreesLessThanHeightRightFrom(int row, int column)
{
	int steps = 1;
	int maxHeight = treeHeights[row][column];
	while (column + steps < width - 1 && treeHeights[row][column + steps] < maxHeight)
		++steps;

	return steps;
}

int[,] scenicScoreMatrix = new int[height, width];


for (int row = 1; row < height - 1; row++)
{
	for (int column = 1; column < width - 1; column++)
	{
		scenicScoreMatrix[row, column] =

			TreesLessThanHeightUpFrom(row, column) *
			TreesLessThanHeightDownFrom(row, column) *
			TreesLessThanHeightLeftFrom(row, column) *
			TreesLessThanHeightRightFrom(row, column);
	}
}

int maxScenicScore = scenicScoreMatrix.ToEnumerable().Max();

Console.WriteLine("Day 8A");
Console.WriteLine($"Number of visible trees: {numberOfVisibleTrees}");

Console.WriteLine("Day 8B");
Console.WriteLine($"Max scenic score: {maxScenicScore}");

public static class ArrayExtensions
{
	public static IEnumerable<T> ToEnumerable<T>(this T[,] target)
	{
		foreach (var item in target)
			yield return item;
	}
}

