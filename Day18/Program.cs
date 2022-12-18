using System.Collections.Immutable;

bool useExampleInput = true;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

IImmutableSet<(int X, int Y, int Z)> allDroplets =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.Select(item => item.Split(','))
		.Select(item => (int.Parse(item[0]), int.Parse(item[1]), int.Parse(item[2])))
		.ToImmutableHashSet();

int surfaceAreaOfAllDroplets =
	allDroplets
		.Sum(item => GetNeighbors(item).Count(neighbor => !allDroplets.Contains(neighbor)));

// One step outside the droplets to ensure all surface areas are visited.
int minX = allDroplets.Min(item => item.X) - 1;
int maxX = allDroplets.Max(item => item.X) + 1;
int minY = allDroplets.Min(item => item.Y) - 1;
int maxY = allDroplets.Max(item => item.Y) + 1;
int minZ = allDroplets.Min(item => item.Z) - 1;
int maxZ = allDroplets.Max(item => item.Z) + 1;

(int X, int Y, int Z) startPosition = (minX, minY, minZ);
ISet<(int X, int Y, int Z)> visitedPositions = new HashSet<(int X, int Y, int Z)>();
ISet<(int X, int Y, int Z)> positionsToVisit = new HashSet<(int X, int Y, int Z)>() { startPosition };
int externalSurfaceAreaOfAllDroplets = 0;

while (positionsToVisit.Any())
{
	(int X, int Y, int Z) currentPosition = positionsToVisit.First();
	positionsToVisit.Remove(currentPosition);
	visitedPositions.Add(currentPosition);

	IEnumerable<(int X, int Y, int Z)> allNeighbors =
		GetNeighbors(currentPosition).Where(IsValidPosition).ToList();
	IEnumerable<(int X, int Y, int Z)> alreadyVisitedNeighbors =
		allNeighbors.Where(neighbor => visitedPositions.Contains(neighbor));
	IEnumerable<(int X, int Y, int Z)> dropletNeighbors =
		allNeighbors.Where(neighbor => allDroplets.Contains(neighbor)).ToList();

	externalSurfaceAreaOfAllDroplets += dropletNeighbors.Count();

	IEnumerable<(int X, int Y, int Z)> neighborsToVisit =
		allNeighbors
			.Except(alreadyVisitedNeighbors)
			.Except(dropletNeighbors);

	foreach ((int X, int Y, int Z) neighbor in neighborsToVisit)
		positionsToVisit.Add(neighbor);
}

Console.WriteLine("Day 18A");
Console.WriteLine($"Surface area of all scanned droplets: {surfaceAreaOfAllDroplets}");

Console.WriteLine("Day 18B");
Console.WriteLine($"External surface area of all scanned droplets: {externalSurfaceAreaOfAllDroplets}");

bool IsValidPosition((int X, int Y, int Z) position) =>
	position.X >= minX && position.X <= maxX &&
	position.Y >= minY && position.Y <= maxY &&
	position.Z >= minZ && position.Z <= maxZ;

IEnumerable<(int X, int Y, int Z)> GetNeighbors((int X, int Y, int Z) position)
{
	yield return (position.X - 1, position.Y, position.Z); // Left
	yield return (position.X + 1, position.Y, position.Z); // Right
	yield return (position.X, position.Y - 1, position.Z); // Down
	yield return (position.X, position.Y + 1, position.Z); // Up
	yield return (position.X, position.Y, position.Z - 1); // Towards
	yield return (position.X, position.Y, position.Z + 1); // Away
}
