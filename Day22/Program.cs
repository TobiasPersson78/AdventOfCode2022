using System.Collections.Immutable;
using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

int tilesPerFaceDimension = useExampleInput
	? 4
	: 50;
const string rotateRight = "R";
const string rotateLeft = "L";
const char solidWall = '#';
const char openTile = '.';
const char notUsedTile = ' ';
char[] wallOrOpenTile = new[] { solidWall, openTile };

const int pointsFactorForRow = 1000;
const int pointsFactorForColumn = 4;
IDictionary<Direction, int> pointsForDirection = new Dictionary<Direction, int>
{
	{ Direction.Left, 2 },
	{ Direction.Up, 3 },
	{ Direction.Right, 0 },
	{ Direction.Down, 1 },
};

IDictionary<Direction, Direction> rotateRightLookup = new Dictionary<Direction, Direction>
{
	{ Direction.Left, Direction.Up },
	{ Direction.Up, Direction.Right },
	{ Direction.Right, Direction.Down },
	{ Direction.Down, Direction.Left },
};
IDictionary<Direction, Direction> rotateLeftLookup = new Dictionary<Direction, Direction>
{
	{ Direction.Left, Direction.Down },
	{ Direction.Up, Direction.Left },
	{ Direction.Right, Direction.Up },
	{ Direction.Down, Direction.Right },
};

ReadState readState = ReadState.ReadingMap;
IList<string> map = new List<string>();
IEnumerable<string> allStepsAndRotations = Enumerable.Empty<string>();

foreach (string line in File.ReadAllLines(inputFilename))
{
	switch ((readState, string.IsNullOrEmpty(line)))
	{
		case (ReadState.ReadingMap, true):
			readState = ReadState.ReadingMovements;
			break;

		case (ReadState.ReadingMap, false):
			map.Add(line);
			break;

		case (ReadState.ReadingMovements, false):
			allStepsAndRotations =
				Regex
					.Matches(line, @"(\d+)|([LR])")
					.Select(match => match.Groups.Values.First().Value)
					.ToList();
			break;
	}
}

// There can be at most four faces in a row or column (although that's not the case for this
// input). For simplicty when checking the borders, fill out the map to have space for four faces
// in each directions.
int maxTilesPerDimensionInMap = 4 * tilesPerFaceDimension;
for (int i = 0; i < map.Count; ++i )
{
	if (map[i].Length < maxTilesPerDimensionInMap)
		map[i] += new string(notUsedTile, maxTilesPerDimensionInMap - map[i].Length);
}
while (map.Count < maxTilesPerDimensionInMap)
{
	map.Add(new string(notUsedTile, maxTilesPerDimensionInMap));
}

int passwordPartA = DoMovement(map, allStepsAndRotations, BorderMovementPartA);
int passwordPartB = DoMovement(map, allStepsAndRotations, BorderMovementPartB);

Console.WriteLine("Day 22A");
Console.WriteLine($"The password is {passwordPartA}.");

Console.WriteLine("Day 21B");
Console.WriteLine($"The password is {passwordPartB}.");

int DoMovement(
	IList<string> map,
	IEnumerable<string> allStepsAndRotations,
	Func<IList<string>, int, int, Direction, (int Row, int Column, Direction Direction)> borderMovement)
{
	int row = 0;
	int column = map[0].IndexOfAny(wallOrOpenTile);
	Direction direction = Direction.Right;

	foreach (string stepsOrRotation in allStepsAndRotations)
	{
		Console.WriteLine($"Position is [{row}, {column}], direction is {direction}, command is {stepsOrRotation}.");

		switch (stepsOrRotation)
		{
			case rotateLeft:
				direction = rotateLeftLookup[direction];
				break;

			case rotateRight:
				direction = rotateRightLookup[direction];
				break;

			default:
				for (int steps = int.Parse(stepsOrRotation); steps > 0; --steps)
				{
					(row, column, direction) = TryMove(map, row, column, direction, borderMovement);
				}
				break;
		}
	}
	Console.WriteLine($"Position is [{row}, {column}], direction is {direction}.");

	int password =
		pointsFactorForRow * (row + 1) +
		pointsFactorForColumn * (column + 1) +
		pointsForDirection[direction];

	return password;
}

(int Row, int Column, Direction Direction) TryMove(
	IList<string> map,
	int row,
	int column,
	Direction direction,
	Func<IList<string>, int, int, Direction, (int Row, int Column, Direction Direction)> borderMovement)
{
	(int xChange, int yChange) = direction switch
	{
		Direction.Left => (-1, 0),
		Direction.Up => (0, -1),
		Direction.Right => (1, 0),
		_ => (0, 1)
	};

	int desiredRow = (row + yChange + map.Count) % map.Count;
	int desiredColumn = (column + xChange + map[row].Length) % map[row].Length;
	Direction desiredDirection = direction;

	if (map[desiredRow][desiredColumn] == notUsedTile)
		(desiredRow, desiredColumn, desiredDirection) = borderMovement(map, row, column, direction);

	if (map[desiredRow][desiredColumn] == solidWall)
		return (row, column, direction);

	return (desiredRow, desiredColumn, desiredDirection);
}

(int Row, int Column, Direction Direction) BorderMovementPartA(
	IList<string> map,
	int row,
	int column,
	Direction direction)
{
	(int desiredRow, int desiredColumn) = (row, column);

	switch (direction)
	{
		case Direction.Left:
			desiredColumn = GetRightEndOfRow(map, row);
			break;

		case Direction.Up:
			desiredRow = GetBottomEndOfColumn(map, column);
			break;

		case Direction.Right:
			desiredColumn = GetLeftStartOfRow(map, row);
			break;

		case Direction.Down:
			desiredRow = GetTopStartOfColumn(map, column);
			break;
	}

	return (desiredRow, desiredColumn, direction);
}

(int Row, int Column, Direction Direction) BorderMovementPartB(
	IList<string> map,
	int row,
	int column,
	Direction direction)
{
	// The map has room for 4x4 faces. This method will only be called for border movements that
	// requires transformation due to the folding of the cube, so not all transformations need to
	// be checked.

	// The faces are counted as they are encounted from the top left corner to the lower right
	// corner, traversing row by row.

	(int desiredRow, int desiredColumn, Direction desiredDirection) = (row, column, direction);

	int faceRow = row / tilesPerFaceDimension;
	int faceColumn = column / tilesPerFaceDimension;
	int rowOnFace = row % tilesPerFaceDimension;
	int columnOnFace = column % tilesPerFaceDimension;

	if (useExampleInput)
	{
		(desiredRow, desiredColumn, desiredDirection) = (faceRow, faceColumn, direction) switch
		{
			(0, 2, Direction.Left) => // Face 1, left side --> Face 3, top side
				(tilesPerFaceDimension, tilesPerFaceDimension + rowOnFace, Direction.Down),
			(0, 2, Direction.Up) => // Face 1, top side --> Face 2, top side
			   (tilesPerFaceDimension, tilesPerFaceDimension - 1 - rowOnFace, Direction.Down),
			(0, 2, Direction.Right) => // Face 1, right side --> Face 6, right side
				(3 * tilesPerFaceDimension - 1 - rowOnFace, 4 * tilesPerFaceDimension - 1, Direction.Left),
			(1, 0, Direction.Left) => // Face 2, left side --> Face 6, bottom side
			   (4 * tilesPerFaceDimension - 1 - rowOnFace, 3 * tilesPerFaceDimension - 1, Direction.Down),
			(1, 0, Direction.Up) => // Face 2, top side --> Face 1, top side
			   (0, 3 * tilesPerFaceDimension - 1 - rowOnFace, Direction.Down),
			(1, 0, Direction.Down) => // Face 2, bottom side --> Face 5, bottom side
				(3 * tilesPerFaceDimension - 1, 3 * tilesPerFaceDimension - 1 - columnOnFace, Direction.Up),
			(1, 1, Direction.Up) => // Face 3, top side --> Face 1, left side
			   (columnOnFace, 2 * tilesPerFaceDimension, Direction.Right),
			(1, 1, Direction.Down) => // Face 3, bottom side --> Face 5, left side
				(3 * tilesPerFaceDimension - columnOnFace, 2 * tilesPerFaceDimension - 1, Direction.Right),
			(1, 2, Direction.Right) => // Face 4, right side --> Face 6, top side
			   (2 * tilesPerFaceDimension, 4 * tilesPerFaceDimension - 1 - rowOnFace, Direction.Down),
			(2, 2, Direction.Left) => // Face 5, left side --> Face 3, bottom side
			   (2 * tilesPerFaceDimension - 1, 2 * tilesPerFaceDimension - 1 - rowOnFace, Direction.Up),
			(2, 2, Direction.Down) => // Face 5, bottom side --> Face 2, bottom side
				(2 * tilesPerFaceDimension - 1, tilesPerFaceDimension - 1 - columnOnFace, Direction.Up),
			(2, 3, Direction.Up) => // Face 6, top side --> Face 4, right side
				(2 * tilesPerFaceDimension - 1 - columnOnFace, 3 * tilesPerFaceDimension - 1, Direction.Left),
			(2, 3, Direction.Right) => // Face 6, right side --> Face 1, right side
			   (tilesPerFaceDimension - 1 - rowOnFace, 3 * tilesPerFaceDimension - 1, Direction.Left),
			(2, 3, Direction.Down) => // Face 6, bottom side --> Face 2, left side
			   (2 * tilesPerFaceDimension - 1 - columnOnFace, 0, Direction.Right),
			_ => throw new InvalidOperationException("Unexpected face and direction.")
		};
	}
	else
	{
		(desiredRow, desiredColumn, desiredDirection) = (faceRow, faceColumn, direction) switch
		{
			(0, 1, Direction.Left) => // Face 1, left side --> Face 4, left side
				(3 * tilesPerFaceDimension - 1 - rowOnFace, 0, Direction.Right),
			(0, 1, Direction.Up) => // Face 1, top side --> Face 6, left side
				(3 * tilesPerFaceDimension + columnOnFace, 0, Direction.Right),
			(0, 2, Direction.Up) => // Face 2, top side --> Face 6, bottom
				(4 * tilesPerFaceDimension - 1, columnOnFace, Direction.Up),
			(0, 2, Direction.Right) => // Face 2, right side --> Face 5, right side
				(3 * tilesPerFaceDimension - 1 - rowOnFace, 2 * tilesPerFaceDimension - 1, Direction.Left),
			(0, 2, Direction.Down) => // Face 2, bottom side --> Face 3, right side
				(tilesPerFaceDimension + columnOnFace, 2 * tilesPerFaceDimension - 1, Direction.Left),
			(1, 1, Direction.Left) => // Face 3, left side --> Face 4, top side
				(2 * tilesPerFaceDimension, rowOnFace, Direction.Down),
			(1, 1, Direction.Right) => // Face 3, right side --> Face 2, bottom side
				(tilesPerFaceDimension - 1, 2 * tilesPerFaceDimension + rowOnFace, Direction.Up),
			(2, 0, Direction.Left) => // Face 4, left side --> Face 1, left side
				(tilesPerFaceDimension - 1 - rowOnFace, tilesPerFaceDimension, Direction.Right),
			(2, 0, Direction.Up) => // Face 4, top side --> Face 3, left side
				(tilesPerFaceDimension + columnOnFace, tilesPerFaceDimension, Direction.Right),
			(2, 1, Direction.Right) => // Face 5, right side --> Face 2, right side
				(tilesPerFaceDimension - 1 - rowOnFace, 3 * tilesPerFaceDimension - 1, Direction.Left),
			(2, 1, Direction.Down) => // Face 5, bottom side --> Face 6, right side
				(3 * tilesPerFaceDimension + columnOnFace, tilesPerFaceDimension - 1, Direction.Left),
			(3, 0, Direction.Left) => // Face 6, left side --> Face 1, top side
				(0, tilesPerFaceDimension + rowOnFace, Direction.Down),
			(3, 0, Direction.Right) => // Face 6, right side --> Face 5, bottom side
				(3 * tilesPerFaceDimension - 1, tilesPerFaceDimension + rowOnFace, Direction.Up),
			(3, 0, Direction.Down) => // Face 6, bottom side --> Face 2, top side
				(0, 2 * tilesPerFaceDimension + columnOnFace, Direction.Down),
			_ => throw new InvalidOperationException("Unexpected face and direction.")
		};
	}

	return (desiredRow, desiredColumn, desiredDirection);
}

int GetLeftStartOfRow(IList<string> map, int row) => map[row].IndexOfAny(wallOrOpenTile);

int GetRightEndOfRow(IList<string> map, int row) => map[row].LastIndexOfAny(wallOrOpenTile);

int GetTopStartOfColumn(IList<string> map, int column) =>
	map
		.Select((row, index) => (RowChar: row[column], Index: index))
		.First(rowCharAndIndex => wallOrOpenTile.Contains(rowCharAndIndex.RowChar))
		.Index;

int GetBottomEndOfColumn(IList<string> map, int column) =>
	map
		.Select((row, index) => (RowChar: row[column], Index: index))
		.Last(rowCharAndIndex => wallOrOpenTile.Contains(rowCharAndIndex.RowChar))
		.Index;

enum ReadState
{
	ReadingMap,
	ReadingMovements
}

enum Direction
{
	Left,
	Up,
	Right,
	Down
}
