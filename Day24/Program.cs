using System.Collections.Immutable;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

const char EmptyTile = '.';
const char LeftTile = '<';
const char UpTile = '^';
const char RightTile = '>';
const char DownTile = 'v';

IList<string> map =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.ToList();
int mapWidth = map.First().Length;
int mapHeight = map.Count;
Position startPosition = new Position(map.First().IndexOf(EmptyTile), 0);
Position targetPosition = new Position(map.Last().IndexOf(EmptyTile), mapHeight - 1);
IDictionary<int, Blizzards> blizzardsForMinuteLookup = new Dictionary<int, Blizzards>
{
	{
		0,
		new Blizzards(
			GetPositionsForTile(map, LeftTile),
			GetPositionsForTile(map, UpTile),
			GetPositionsForTile(map, RightTile),
			GetPositionsForTile(map, DownTile))
	}
};
IList<Movement> allMovements = new List<Movement>
{
	new Movement(-1, 0), // Left
	new Movement(0, -1), // Up
	new Movement(1, 0), // Right
	new Movement(0, 1), // Down
	new Movement(0, 0), // No movement
};

int numberOfMinutesRequiredPartA = DoBreadthFirstSearchThroughBlizzard(
	startPosition,
	0,
	targetPosition);
int numberOfMinutesToTargetAndBack = DoBreadthFirstSearchThroughBlizzard(
	targetPosition,
	numberOfMinutesRequiredPartA,
	startPosition);
int numberOfMinutesTargetStartTargetPartB = DoBreadthFirstSearchThroughBlizzard(
	startPosition,
	numberOfMinutesToTargetAndBack,
	targetPosition);

Console.WriteLine("Day 24A");
Console.WriteLine($"The minimum number of minutes required is {numberOfMinutesRequiredPartA}.");

Console.WriteLine("Day 24B");
Console.WriteLine($"The minimum number of minutes required from start to target, to start, and to target again is {numberOfMinutesTargetStartTargetPartB}.");

int DoBreadthFirstSearchThroughBlizzard(Position startPosition, int startMinutes, Position targetPosition)
{
	BfsSearchState startState = new BfsSearchState(startPosition, startMinutes);
	Queue<BfsSearchState> queueForBreadthFirstSearch = new Queue<BfsSearchState>();
	queueForBreadthFirstSearch.Enqueue(startState);
	ISet<BfsSearchState> setOfVisitedStatesForBFS = new HashSet<BfsSearchState>
	{
		startState
	};

	while (true)
	{
		(Position currentPosition, int currentMinutes) = queueForBreadthFirstSearch.Dequeue();

		if (currentPosition == targetPosition)
		{
			return currentMinutes;
		}

		++currentMinutes;
		foreach (Position positionToTry
			in allMovements
				.Select(item =>
					currentPosition with
					{
						X = currentPosition.X + item.DeltaX,
						Y = currentPosition.Y + item.DeltaY
					})
				.Where(item => IsValidPosition(item, currentMinutes)))
		{
			BfsSearchState stateToTry = new BfsSearchState(positionToTry, currentMinutes);
			if (setOfVisitedStatesForBFS.Contains(stateToTry))
				continue;

			setOfVisitedStatesForBFS.Add(stateToTry);
			queueForBreadthFirstSearch.Enqueue(new BfsSearchState(positionToTry, currentMinutes));
		}
	}
}

bool IsValidPosition(Position position, int minute)
{
	if (position == startPosition)
		return true;

	if (position == targetPosition)
		return true;

	if (position.X <= 0 || position.X >= mapWidth - 1 ||
		position.Y <= 0 || position.Y >= mapHeight -1)
	{
		return false;
	}

	Blizzards blizzards = GetBlizzardsForMinute(minute);

	if (blizzards.ToLeft.Contains(position) ||
		blizzards.ToUp.Contains(position) ||
		blizzards.ToRight.Contains(position) ||
		blizzards.ToDown.Contains(position))
	{
		return false;
	}

	return true;
}

IImmutableSet<Position> GetPositionsForTile(IList<string> map, char tile) =>
	map
		.SelectMany((row, rowIndex) =>
			row
				.Select((charInRow, columnIndex) => (Char: charInRow, Column: columnIndex))
				.Where(item => item.Char == tile)
				.Select(item => new Position(item.Column, rowIndex)))
		.ToImmutableHashSet();

Blizzards GetBlizzardsForMinute(int minute)
{
	if (blizzardsForMinuteLookup.TryGetValue(minute, out Blizzards? blizzards))
		return blizzards!;

	blizzards = GetBlizzardsForMinute(minute - 1);
	blizzards = MoveBlizzards(blizzards);
	blizzardsForMinuteLookup[minute] = blizzards;

	return blizzards;
}

Blizzards MoveBlizzards(Blizzards blizzards) =>
	new Blizzards(
		blizzards.ToLeft.Select(item => item with { X = item.X > 1 ? item.X - 1 : mapWidth - 2 }).ToImmutableHashSet(),
		blizzards.ToUp.Select(item => item with { Y = item.Y > 1 ? item.Y - 1 : mapHeight - 2 }).ToImmutableHashSet(),
		blizzards.ToRight.Select(item => item with { X = item.X < mapWidth - 2 ? item.X + 1 : 1 }).ToImmutableHashSet(),
		blizzards.ToDown.Select(item => item with { Y = item.Y < mapHeight - 2 ? item.Y + 1 : 1 }).ToImmutableHashSet());

record Position(int X, int Y);
record Movement(int DeltaX, int DeltaY);
record Blizzards(
	IImmutableSet<Position> ToLeft,
	IImmutableSet<Position> ToUp,
	IImmutableSet<Position> ToRight,
	IImmutableSet<Position> ToDown);
record BfsSearchState(Position Position, int Minutes);
