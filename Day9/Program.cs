bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

const int maxAllowedDistance = 1;

(int X, int Y) headPosition = (0, 0);
(int X, int Y) tailPosition = headPosition;
IList<(int X, int Y)> chainPositions = new List<(int X, int Y)>
{
	(0, 0),
	(0, 0),
	(0, 0),
	(0, 0),
	(0, 0),
	(0, 0),
	(0, 0),
	(0, 0),
	(0, 0),
	(0, 0),
};

static (int X, int Y) MoveNextToIfNeeded((int X, int Y) firstPosition, (int X, int Y) secondPosition)
{
	int horizontalDistance = firstPosition.X - secondPosition.X;
	int verticalDistance = firstPosition.Y - secondPosition.Y;

	// Left up
	if (horizontalDistance < -maxAllowedDistance && verticalDistance > maxAllowedDistance)
		return (firstPosition.X + 1, firstPosition.Y - 1);
	// Right up
	if (horizontalDistance > maxAllowedDistance && verticalDistance > maxAllowedDistance)
		return (firstPosition.X - 1, firstPosition.Y - 1);
	// Right down
	if (horizontalDistance > maxAllowedDistance && verticalDistance < -maxAllowedDistance)
		return (firstPosition.X - 1, firstPosition.Y + 1);
	// Left down
	if (horizontalDistance < -maxAllowedDistance && verticalDistance < -maxAllowedDistance)
		return (firstPosition.X + 1, firstPosition.Y + 1);
	// Left
	if (horizontalDistance < -maxAllowedDistance)
		return (firstPosition.X + 1, firstPosition.Y);
	// Right
	if (horizontalDistance > maxAllowedDistance)
		return (firstPosition.X - 1, firstPosition.Y);
	// Up
	if (verticalDistance > maxAllowedDistance)
		return (firstPosition.X, firstPosition.Y - 1);
	// Down
	if (verticalDistance < -maxAllowedDistance)
		return (firstPosition.X, firstPosition.Y + 1);

	return secondPosition;
}

ISet<(int X, int Y)> setOfTailPositionsPartA = new HashSet<(int X, int Y)>
{
	tailPosition
};
ISet<(int X, int Y)> setOfTailPositionsPartB = new HashSet<(int X, int Y)>
{
	tailPosition
};


foreach (string row in File.ReadAllLines(inputFilename).Where(item => !string.IsNullOrEmpty(item)))
{
	char direction = row[0];

	for (int steps = int.Parse(row[2..]); steps > 0; steps--)
	{
		switch (direction)
		{
			case 'L':
				headPosition.X--;
				break;
			case 'R':
				headPosition.X++;
				break;
			case 'U':
				headPosition.Y++;
				break;
			case 'D':
				headPosition.Y--;
				break;
			default:
				throw new InvalidOperationException("Invalid direction.");
		}

		tailPosition = MoveNextToIfNeeded(headPosition, tailPosition);
		setOfTailPositionsPartA.Add(tailPosition);

		chainPositions[0] = headPosition;
		for (int i = 1; i < chainPositions.Count; ++i)
		{
			chainPositions[i] = MoveNextToIfNeeded(chainPositions[i - 1], chainPositions[i]);
		}

		setOfTailPositionsPartB.Add(chainPositions.Last());
	}
}

int numberOfVisitedPositionsPartA = setOfTailPositionsPartA.Count;
int numberOfVisitedPositionsPartB = setOfTailPositionsPartB.Count;

Console.WriteLine("Day 9A");
Console.WriteLine($"Number of visited positions: {numberOfVisitedPositionsPartA}");

Console.WriteLine("Day 9B");
Console.WriteLine($"Number of visited positions: {numberOfVisitedPositionsPartB}");
