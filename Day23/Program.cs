bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

const char elfPositionIndicator = '#';

ISet<Position> positionsOfElves =
	File
		.ReadAllLines(inputFilename)
		.SelectMany((row, rowIndex) =>
			row
				.Select((charInRow, columnIndex) => (Char: charInRow, Column: columnIndex))
				.Where(item => item.Char == elfPositionIndicator)
				.Select(item => new Position(item.Column, rowIndex)))
		.ToHashSet();

IList<Func<Position, IEnumerable<Position>, (bool Move, Position Target)>> movementProposals =
	new List<Func<Position, IEnumerable<Position>, (bool Move, Position Target)>>
	{
		ProposeMoveNorth,
		ProposeMoveSouth,
		ProposeMoveWest,
		ProposeMoveEast
	};

int coveredAreaWithoutElvesPartA = 0;
int roundsBeforeNoMovementPartB = 0;
const int numberOfRoundsPartA = 10;

for (int roundIndex = 0; ; ++roundIndex)
{
	if (roundIndex == numberOfRoundsPartA)
	{
		int minX = positionsOfElves.Min(item => item.X);
		int maxX = positionsOfElves.Max(item => item.X);
		int minY = positionsOfElves.Min(item => item.Y);
		int maxY = positionsOfElves.Max(item => item.Y);

		coveredAreaWithoutElvesPartA =
			(maxX - minX + 1) * (maxY - minY + 1) -
			positionsOfElves.Count(item =>
				item.X >= minX && item.X <= maxX &&
				item.Y >= minY && item.Y <= maxY);
	}

	IDictionary<Position, IList<Position>> proposedMovements = new Dictionary<Position, IList<Position>>();

	foreach (Position currentElf in positionsOfElves)
	{
		ICollection<Position> neighbors = GetNeighbors(currentElf).ToList();

		if (neighbors.Any())
		{
			foreach (var movementProposal in movementProposals)
			{
				(bool canMove, Position newPosition) = movementProposal(currentElf, neighbors);

				if (canMove)
				{
					if (proposedMovements.TryGetValue(newPosition, out IList<Position>? elvesForPosition))
						elvesForPosition!.Add(currentElf);
					else
						proposedMovements[newPosition] = new List<Position> { currentElf };

					break;
				}
			}
		}
	}

	bool anyMovement = false;
	foreach (KeyValuePair<Position, IList<Position>> positionAndIndices
		in proposedMovements.Where(positionAndIndices => positionAndIndices.Value.Count == 1))
	{
		positionsOfElves.Remove(positionAndIndices.Value.First());
		positionsOfElves.Add(positionAndIndices.Key);
		anyMovement = true;
	}

	if (!anyMovement)
	{
		roundsBeforeNoMovementPartB = roundIndex + 1;
		break;
	}

	var previousFirstProposal = movementProposals.First();
	movementProposals.RemoveAt(0);
	movementProposals.Add(previousFirstProposal);
}

Console.WriteLine("Day 23A");
Console.WriteLine($"The covered area is {coveredAreaWithoutElvesPartA} squares.");

Console.WriteLine("Day 23B");
Console.WriteLine($"Number of rounds before no movement: {roundsBeforeNoMovementPartB}");

IEnumerable<Position> GetNeighbors(Position position) =>
	new[]
	{
		position with { X = position.X - 1, Y = position.Y - 1 },
		position with { Y = position.Y - 1 },
		position with { X = position.X + 1, Y = position.Y - 1 },

		position with { X = position.X - 1 },
		position with { X = position.X + 1 },

		position with { X = position.X - 1, Y = position.Y + 1 },
		position with { Y = position.Y + 1 },
		position with { X = position.X + 1, Y = position.Y + 1 },
	}.Where(positionsOfElves.Contains);

(bool Move, Position Target) ProposeMoveWest(Position position, IEnumerable<Position> neighbors) =>
	neighbors.Any(item => item.X == position.X - 1 && item.Y >= position.Y - 1 && item.Y <= position.Y + 1)
		? (false, position)
		: (true, position with { X = position.X - 1 });

(bool Move, Position Target) ProposeMoveNorth(Position position, IEnumerable<Position> neighbors) =>
	neighbors.Any(item => item.Y == position.Y - 1 && item.X >= position.X - 1 && item.X <= position.X + 1)
		? (false, position)
		: (true, position with { Y = position.Y - 1 });

(bool Move, Position Target) ProposeMoveEast(Position position, IEnumerable<Position> neighbors) =>
	neighbors.Any(item => item.X == position.X + 1 && item.Y >= position.Y - 1 && item.Y <= position.Y + 1)
		? (false, position)
		: (true, position with { X = position.X + 1 });

(bool Move, Position Target) ProposeMoveSouth(Position position, IEnumerable<Position> neighbors) =>
	neighbors.Any(item => item.Y == position.Y + 1 && item.X >= position.X - 1 && item.X <= position.X + 1)
		? (false, position)
		: (true, position with { Y = position.Y + 1 });

record Position(int X, int Y);
