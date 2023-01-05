using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

const string RotateRight = "R";
const string RotateLeft = "L";
const char SolidWall = '#';
const char OpenTile = '.';
const char NotUsedTile = ' ';

const int PointsFactorForRow = 1000;
const int PointsFactorForColumn = 4;
Dictionary<Direction, int> pointsForDirection = new()
{
	{ Direction.Left, 2 },
	{ Direction.Up, 3 },
	{ Direction.Right, 0 },
	{ Direction.Down, 1 },
};

Dictionary<(Direction Direction, Rotation Rotation), Direction> rotationLookup = new()
{
	{ (Direction.Left, Rotation.None), Direction.Left },
	{ (Direction.Left, Rotation.Degrees90), Direction.Up },
	{ (Direction.Left, Rotation.Degrees180), Direction.Right },
	{ (Direction.Left, Rotation.Degrees270), Direction.Down },

	{ (Direction.Up, Rotation.None), Direction.Up },
	{ (Direction.Up, Rotation.Degrees90), Direction.Right },
	{ (Direction.Up, Rotation.Degrees180), Direction.Down },
	{ (Direction.Up, Rotation.Degrees270), Direction.Left },

	{ (Direction.Right, Rotation.None), Direction.Right },
	{ (Direction.Right, Rotation.Degrees90), Direction.Down },
	{ (Direction.Right, Rotation.Degrees180), Direction.Left },
	{ (Direction.Right, Rotation.Degrees270), Direction.Up },

	{ (Direction.Down, Rotation.None), Direction.Down },
	{ (Direction.Down, Rotation.Degrees90), Direction.Left },
	{ (Direction.Down, Rotation.Degrees180), Direction.Up },
	{ (Direction.Down, Rotation.Degrees270), Direction.Right },
};

IDictionary<Direction, (int XChange, int YChange)> xyChangeForDirectionLookup = new Dictionary<Direction, (int XChange, int YChange)>
{
	{ Direction.Left, (-1, 0) },
	{ Direction.Up, (0, -1) },
	{ Direction.Right, (1, 0) },
	{ Direction.Down, (0, 1) },
};

// Due to the way the files are saved, input.txt and exampleInput.txt have different line endings,
// so splitting by Environment.Newline does not work. But the file can be split into two section as
// the content is separated by an empty line, i.e. where two new lines follow each other.
IList<string> inputFileSections =
	File
		.ReadAllText(inputFilename)
		.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None);
IList<string> map = inputFileSections.First().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
IEnumerable<string> allStepsAndRotations =
	Regex
		.Matches(inputFileSections.Last(), @"(\d+)|([LR])")
		.Select(match => match.Groups.Values.First().Value)
		.ToList();

int tileRowsInMap = map.Count;
int tileColumnsInMap = map.Max(row => row.Length);

// For simplicty when checking the map, fill out the rows to have the same length.
for (int i = 0; i < tileRowsInMap; ++i )
	map[i] += new string(NotUsedTile, tileColumnsInMap - map[i].Length);

// The number of used tiles divided by six is the number of tiles used for each face. The square
// root of that becomes the size of the face width and height.
const int NumberOfCubeFaces = 6;
int tilesPerFaceDimension =
	Convert.ToInt32(
		Math.Round(
			Math.Sqrt(
				map.Sum(row =>
					row.Count(column => column != NotUsedTile)) / NumberOfCubeFaces)));

int faceRowsInMap = tileRowsInMap / tilesPerFaceDimension;
int faceColumnsInMap = tileColumnsInMap / tilesPerFaceDimension;

IList<(int FaceRow, int FaceColumn)> facePositionsInMap =
	Enumerable
		.Range(0, faceRowsInMap)
		.SelectMany(faceRowIndex =>
			Enumerable
				.Range(0, faceColumnsInMap)
				.Where(faceColumnIndex =>
					map[faceRowIndex * tilesPerFaceDimension][faceColumnIndex * tilesPerFaceDimension] != NotUsedTile)
				.Select(faceColumnIndex => (FaceRow: faceRowIndex, FaceColumn: faceColumnIndex)))
		.ToList();

// Breadth-first search to find faces.
Dictionary<(int FaceRow, int FaceColumn), Face> facePositionToFaceLookup = new();
// Due to rotation of the faces, use the location instead of the face itself as key.
Dictionary<FaceLocation, (int FaceRow, int FaceColumn)> faceLocationToMapFacePositionLookup = new();
Queue<(int FaceRow, int FaceColumn, Face Face)> queue = new();
// Any cube face would work as starting face; it's the relative position of the faces that is of
// interest.
Face startFace = new Face(0b000, 0b001, 0b010, 0b011); // The front face.
(int startFaceRow, int startFaceColumn) = facePositionsInMap.First();
queue.Enqueue((startFaceRow, startFaceColumn, startFace));
while (queue.Any())
{
	(int faceRow, int faceColumn, Face face) = queue.Dequeue();

	if (facePositionToFaceLookup.ContainsKey((faceRow, faceColumn)))
		continue;

	foreach (Direction direction in Enum.GetValues<Direction>())
	{
		(int xChange, int yChange) = xyChangeForDirectionLookup[direction];

		int rowInDirection = faceRow + yChange;
		int columnInDirection = faceColumn + xChange;

		if (facePositionsInMap.Contains((rowInDirection, columnInDirection)))
			queue.Enqueue((rowInDirection, columnInDirection, face.GetFaceInDirection(direction)));
	}

	facePositionToFaceLookup[(faceRow, faceColumn)] = face;
	faceLocationToMapFacePositionLookup[face.GetLocation()] = (faceRow, faceColumn);
}

var faceTransitions =
	new Dictionary<(int FaceRow, int FaceColumn, Direction Direction),
		(int FaceRow, int FaceColumn, Rotation Rotation)>();
foreach ((int FaceRow, int FaceColumn) facePosition in facePositionsInMap)
{
	Face face = facePositionToFaceLookup[facePosition];

	foreach (Direction direction in Enum.GetValues<Direction>())
	{
		Face faceInDirection = face.GetFaceInDirection(direction);
		(int FaceRow, int FaceColumn) facePositionInDirection =
			faceLocationToMapFacePositionLookup[faceInDirection.GetLocation()];

		Rotation rotationInDirection =
			faceInDirection.GetRotationToFace(facePositionToFaceLookup[facePositionInDirection]);

		faceTransitions[(facePosition.FaceRow, facePosition.FaceColumn, direction)] =
			(facePositionInDirection.FaceRow, facePositionInDirection.FaceColumn, rotationInDirection);
	}
}

int passwordPartA = DoMovement(map, allStepsAndRotations, GetNextTilePositionPartA);
int passwordPartB = DoMovement(map, allStepsAndRotations, GetNextTilePositionPartB);

Console.WriteLine("Day 22A");
Console.WriteLine($"The password is {passwordPartA}.");

Console.WriteLine("Day 22B");
Console.WriteLine($"The password is {passwordPartB}.");

int DoMovement(
	IList<string> map,
	IEnumerable<string> allStepsAndRotations,
	Func<IList<string>, int, int, Direction, (int Row, int Column, Direction Direction)> getNextTilePosition)
{
	int row = 0;
	int column = map[0].IndexOf(OpenTile);
	Direction direction = Direction.Right;

	foreach (string stepsOrRotation in allStepsAndRotations)
	{
		Console.WriteLine($"Position is [{row}, {column}], direction is {direction}, command is {stepsOrRotation}.");

		switch (stepsOrRotation)
		{
			case RotateLeft:
				direction = rotationLookup[(direction, Rotation.Degrees270)];
				break;

			case RotateRight:
				direction = rotationLookup[(direction, Rotation.Degrees90)];
				break;

			default:
				for (int steps = int.Parse(stepsOrRotation); steps > 0; --steps)
				{
					(int desiredRow, int desiredColumn, Direction desiredDirection) =
						getNextTilePosition(map, row, column, direction);

					if (map[desiredRow][desiredColumn] == SolidWall)
						break; // No more steps in this direction is possible.

					(row, column, direction) = (desiredRow, desiredColumn, desiredDirection);
				}
				break;
		}
	}
	Console.WriteLine($"Position is [{row}, {column}], direction is {direction}.");

	int password =
		PointsFactorForRow * (row + 1) +
		PointsFactorForColumn * (column + 1) +
		pointsForDirection[direction];

	return password;
}

(int Row, int Column, Direction Direction) GetNextTilePositionPartA(
	IList<string> map,
	int row,
	int column,
	Direction direction)
{
	(int xChange, int yChange) = xyChangeForDirectionLookup[direction];

	int desiredRow = row;
	int desiredColumn = column;

	do
	{
		// Wrap around and continue moving until a tile that is used is encountered.
		desiredRow = (desiredRow + yChange + tileRowsInMap) % tileRowsInMap;
		desiredColumn = (desiredColumn + xChange + tileColumnsInMap) % tileColumnsInMap;
	}
	while (map[desiredRow][desiredColumn] == NotUsedTile);

	return (desiredRow, desiredColumn, direction);
}

(int Row, int Column, Direction Direction) GetNextTilePositionPartB(
	IList<string> map,
	int row,
	int column,
	Direction direction)
{
	int faceRow = row / tilesPerFaceDimension;
	int faceColumn = column / tilesPerFaceDimension;
	int rowOnFace = row % tilesPerFaceDimension;
	int columnOnFace = column % tilesPerFaceDimension;

	(int xChange, int yChange) = xyChangeForDirectionLookup[direction];

	if (rowOnFace + yChange >= 0 && rowOnFace + yChange < tilesPerFaceDimension &&
		columnOnFace + xChange >= 0 && columnOnFace + xChange < tilesPerFaceDimension)
	{
		// There isn't a transition to another face.
		return (row + yChange, column + xChange, direction);
	}

	// There is a transition to another face.
	(int newFaceRow, int newFaceColumn, Rotation rotation) = faceTransitions[(faceRow, faceColumn, direction)];
	int unrotatedRowOnNewFace = (row + yChange + tilesPerFaceDimension) % tilesPerFaceDimension;
	int unrotatedColumnOnNewFace = (column + xChange + tilesPerFaceDimension) % tilesPerFaceDimension;
	(int rotatedColumnOnNewFace, int rotatedRowOnNewFace) = RotateSquareCoordinate(
		unrotatedColumnOnNewFace,
		unrotatedRowOnNewFace,
		rotation,
		tilesPerFaceDimension);
	int desiredRow = newFaceRow * tilesPerFaceDimension + rotatedRowOnNewFace;
	int desiredColumn = newFaceColumn * tilesPerFaceDimension + rotatedColumnOnNewFace;
	Direction desiredDirection = rotationLookup[(direction, rotation)];

	return (desiredRow, desiredColumn, desiredDirection);
}

(int X, int Y) RotateSquareCoordinate(int x, int y, Rotation rotation, int squareDimension) =>
	rotation switch
	{
		Rotation.None => (x, y),
		Rotation.Degrees90 => (squareDimension - 1 - y, x),
		Rotation.Degrees180 => (squareDimension - 1 - x, squareDimension - 1 - y),
		Rotation.Degrees270 => (y, squareDimension - 1 - x),
		_ => throw new InvalidOperationException("Unexpected rotation.")
	};

enum Direction
{
	Left,
	Up,
	Right,
	Down
}

enum Rotation
{
	/// <summary>
	/// No rotation.
	/// </summary>
	None,
	/// <summary>
	/// 90 degrees clockwise rotation, "right".
	/// </summary>
	Degrees90,
	/// <summary>
	/// 180 degrees rotation.
	/// </summary>
	Degrees180,
	/// <summary>
	/// 90 degrees counter-clockwise rotation (i.e. 270 degrees clockwise rotation), "left".
	/// </summary>
	Degrees270
}

enum FaceLocation
{
	Front,
	Left,
	Top,
	Bottom,
	Right,
	Back
}

/// <summary>
/// A face of a 3D cube, decribed by four vertices. Here, each vertex is identified by a single
/// number where the least significant three bits signals the X, Y and Z position of the vertex'
/// location on a unit sized cube.
/// </summary>
record class Face(int TopLeft, int TopRight, int BottomLeft, int BottomRight)
{
	private const int XMask = 0b001;
	private const int YMask = 0b010;
	private const int ZMask = 0b100;
	private const int XYZ = 0b111;

	/// <summary>
	/// Gets the location of the face.
	/// </summary>
	/// <returns>The location of the face.</returns>
	/// <remarks>
	///	  Due to rotations, there are faces with the same location that does not have the same vertex
	///	  values.
	///	</remarks>
	public FaceLocation GetLocation()
	{
		int[] vertices = new[] { TopLeft, TopRight, BottomLeft, BottomRight };

		FaceLocation? AreEqual(Func<int, int> conversion, FaceLocation zeroMatch, FaceLocation nonZeroMatch)
		{
			int min = vertices.Min(conversion);
			int max = vertices.Max(conversion);

			if (min == max)
				return min == 0 ? zeroMatch : nonZeroMatch;

			return null;
		}

		return
			AreEqual(position => (position & XMask) >> 0, FaceLocation.Left, FaceLocation.Right) ??
			AreEqual(position => (position & YMask) >> 0, FaceLocation.Bottom, FaceLocation.Top) ??
			AreEqual(position => (position & ZMask) >> 0, FaceLocation.Front, FaceLocation.Back) ??
			throw new InvalidOperationException("Unexpected face coordinates.");
	}

	/// <summary>
	/// Gets a face in the specified direction from this face.
	/// </summary>
	/// <param name="direction">The direction to get the face in.</param>
	/// <returns>A face in the specified direction.</returns>
	/// <remarks>
	/// Each vertex is a number in the range from 0b000 to 0b111, and any vertex plus its three
	/// neighboring vertices adds up to 0b111. That is used backwards here - given that we know
	/// that the sum will be 0b111, we know the corner that is rotated, and we know two neighbors
	/// of that corner, we can determine the position of the third neighbor through bit arithmetic.
	/// </remarks>
	public Face GetFaceInDirection(Direction direction) =>
			direction switch
			{
				Direction.Left => new Face(
					XYZ ^ TopLeft ^ TopRight ^ BottomLeft,
					TopLeft,
					XYZ ^ TopLeft ^ BottomLeft ^ BottomRight,
					BottomLeft),
				Direction.Up => new Face(
					XYZ ^ TopLeft ^ TopRight ^ BottomLeft,
					XYZ ^ TopLeft ^ TopRight ^ BottomRight,
					TopLeft,
					TopRight),
				Direction.Right => new Face(
					TopRight,
					XYZ ^ TopLeft ^ TopRight ^ BottomRight,
					BottomRight,
					XYZ ^ TopRight ^ BottomLeft ^ BottomRight),
				Direction.Down => new Face(
					BottomLeft,
					BottomRight,
					XYZ ^ TopLeft ^ BottomLeft ^ BottomRight,
					XYZ ^ TopRight ^ BottomLeft ^ BottomRight),
				_ => throw new InvalidOperationException("Unexpected direction")
			};

	public Rotation GetRotationToFace(Face rotatedFace)
	{
		if (TopLeft == rotatedFace.TopLeft) return Rotation.None;
		if (TopLeft == rotatedFace.TopRight) return Rotation.Degrees90;
		if (TopLeft == rotatedFace.BottomRight) return Rotation.Degrees180;
		if (TopLeft == rotatedFace.BottomLeft) return Rotation.Degrees270;

		throw new ArgumentException("The rotated face is not a rotation of this face.");
	}

	public sealed override string ToString()
	{
		return GetLocation().ToString();
	}
}
