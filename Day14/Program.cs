bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

const bool DisplayProgress = true;
const char Air = '.';
const char Stone = '#';
const char Sand = 'o';
const char Drop = '+';
const char Falling = '~';
TimeSpan fallSpeed = TimeSpan.FromMilliseconds(0);
const int DropXPositionBeforeTransformation = 500;

int numberOfGrainsOfSandBeforeOverflow = await CountFallingGrainsOfSandsAsync(inputFilename, false);
int numberOfGrainsOfSandBeforeStop = await CountFallingGrainsOfSandsAsync(inputFilename, true);

Console.WriteLine("Day 14A");
Console.WriteLine($"Number of grains of sand before overflow: {numberOfGrainsOfSandBeforeOverflow}");

Console.WriteLine("Day 14B");
Console.WriteLine($"Number of grains of sand before stop: {numberOfGrainsOfSandBeforeStop}");

async Task<int> CountFallingGrainsOfSandsAsync(string inputFilename, bool addFloor)
{
	List<List<(int X, int Y)>> allLinesUntransformed =
		File
			.ReadAllLines(inputFilename)
			.Where(item => !string.IsNullOrEmpty(item))
			.Select(row =>
				row
					.Split(" -> ")
					.Select(positionString =>
					{
						string[] xy = positionString.Split(',');
						return (X: int.Parse(xy[0]), Y: int.Parse(xy[1]));
					})
					.ToList())
			.ToList();
    int yMax = allLinesUntransformed.Max(item => item.Max(xy => xy.Y));

	if (addFloor)
	{
		yMax += 2;
		const int ExtraWidthToSimulateInfinity = 5;
		allLinesUntransformed.Add(new List<(int X, int Y)>
		{
			(DropXPositionBeforeTransformation - yMax - ExtraWidthToSimulateInfinity, yMax),
            (DropXPositionBeforeTransformation + yMax + ExtraWidthToSimulateInfinity, yMax),
        });
	}
    
	int xMin = allLinesUntransformed.Min(item => item.Min(xy => xy.X));
	int xMax = allLinesUntransformed.Max(item => item.Max(xy => xy.X));
	// Add empty spaces at the sides
	int xOffset = xMin - 1;
	int rowWidth = xMax - xMin + 3;

	if (DisplayProgress)
	{
		int windowWidth = Math.Max(Console.WindowWidth, rowWidth + 1);
		int windowHeight = Math.Max(Console.WindowHeight, yMax + 5);
		Console.SetWindowSize(windowWidth, windowHeight);
		Console.CursorVisible = false;
	}

	List<List<(int X, int Y)>> allLines =
		allLinesUntransformed
			.Select(line =>
				line
					.Select(xy => (X: xy.X - xOffset, xy.Y))
					.ToList())
			.ToList();
	List<char[]> grid =
		Enumerable
			.Range(0, yMax + 1)
			.Select(_ => Enumerable.Repeat(Air, rowWidth).ToArray())
			.ToList();
	IEnumerable<(int X, int Y)> filledPositions =
		allLines
			.SelectMany(line =>
				line
					.Aggregate(
						new
						{
							FilledPoints = new List<(int X, int Y)>(),
							PreviousCoordinate = default((int X, int Y))
						},
						(filledPointsAndPreviousCoordinate, currentCoordinate) =>
						{
							if (filledPointsAndPreviousCoordinate.PreviousCoordinate != default)
							{
								for (int x = Math.Min(filledPointsAndPreviousCoordinate.PreviousCoordinate.X, currentCoordinate.X);
									x <= Math.Max(filledPointsAndPreviousCoordinate.PreviousCoordinate.X, currentCoordinate.X);
									++x)
								{
									for (int y = Math.Min(filledPointsAndPreviousCoordinate.PreviousCoordinate.Y, currentCoordinate.Y);
										y <= Math.Max(filledPointsAndPreviousCoordinate.PreviousCoordinate.Y, currentCoordinate.Y);
										++y)
									{
										filledPointsAndPreviousCoordinate.FilledPoints.Add((X: x, Y: y));
									}
								}
							}
							return new
							{
								filledPointsAndPreviousCoordinate.FilledPoints,
								PreviousCoordinate = currentCoordinate
							};
						},
						filledPointsAndPreviousCoordinate => filledPointsAndPreviousCoordinate.FilledPoints));
	foreach (var xy in filledPositions)
	{
		grid[xy.Y][xy.X] = Stone;
	}

	(int X, int Y) dropPosition = (DropXPositionBeforeTransformation - xOffset, 0);
	grid[dropPosition.Y][dropPosition.X] = Drop;

    if (DisplayProgress)
        PrintGrid(grid);

    int numberOfGrainsOfSandThatHasFallen = 0;
	while (true)
	{
		(int X, int Y) sandPosition = dropPosition;

		do
		{
			(int X, int Y) newPosition = MoveSand(sandPosition, grid);

			if (newPosition != sandPosition)
			{
				grid[sandPosition.Y][sandPosition.X] = Air;
				grid[newPosition.Y][newPosition.X] = Falling;
				grid[dropPosition.Y][dropPosition.X] = Drop;

				if (DisplayProgress)
				{
					PrintGridPosition(grid, newPosition);
					PrintGridPosition(grid, sandPosition);
					PrintGridPosition(grid, dropPosition);
				}
                
				sandPosition = newPosition;
				continue;
			}

			grid[sandPosition.Y][sandPosition.X] = Sand;

			if (DisplayProgress)
			{
				PrintGridPosition(grid, newPosition);
				PrintGridPosition(grid, sandPosition);
				PrintGridPosition(grid, dropPosition);
				await Task.Delay(fallSpeed);
			}

			break;
		}
		while (sandPosition.Y < yMax);

		if (sandPosition.Y >= yMax)
			break;

		++numberOfGrainsOfSandThatHasFallen;

		if (sandPosition == dropPosition)
			break;
	}

    if (DisplayProgress)
        PrintGrid(grid);

	return numberOfGrainsOfSandThatHasFallen;
}

(int X, int Y) MoveSand((int X, int Y) sandPosition, List<char[]> grid)
{
	// Can it move straight down?
	if (grid[sandPosition.Y + 1][sandPosition.X] == Air)
		return (sandPosition.X, sandPosition.Y + 1);

	// Can it move diagonally down to the left?
	if (grid[sandPosition.Y + 1][sandPosition.X - 1] == Air)
		return (sandPosition.X - 1, sandPosition.Y + 1);

    // Can it move diagonally down to the right?
    if (grid[sandPosition.Y + 1][sandPosition.X + 1] == Air)
        return (sandPosition.X + 1, sandPosition.Y + 1);

    // It could not move.
    return sandPosition;
}

void PrintGridPosition(List<char[]> grid, (int X, int Y) position)
{
	Console.SetCursorPosition(position.X, position.Y);
	Console.Write(grid[position.Y][position.X]);
}

void PrintGrid(List<char[]> grid)
{
	Console.SetCursorPosition(0, 0);
	foreach (char[] line in grid)
		Console.WriteLine(line);
}