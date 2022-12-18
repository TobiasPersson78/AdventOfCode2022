using System.Collections.Immutable;

bool useExampleInput = true;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

IImmutableSet<Point3D> allDroplets =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.Select(item => item.Split(','))
		.Select(item => new Point3D(int.Parse(item[0]), int.Parse(item[1]), int.Parse(item[2])))
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

Point3D startPosition = new(minX, minY, minZ);
ISet<Point3D> visitedPositions = new HashSet<Point3D>();
ISet<Point3D> positionsToVisit = new HashSet<Point3D>() { startPosition };
int externalSurfaceAreaOfAllDroplets = 0;

while (positionsToVisit.Any())
{
	Point3D currentPosition = positionsToVisit.First();
	positionsToVisit.Remove(currentPosition);
	visitedPositions.Add(currentPosition);

	IEnumerable<Point3D> allNeighbors = GetNeighbors(currentPosition).Where(IsValidPosition).ToList();
	IEnumerable<Point3D> alreadyVisitedNeighbors = allNeighbors.Where(neighbor => visitedPositions.Contains(neighbor));
	IEnumerable<Point3D> dropletNeighbors = allNeighbors.Where(neighbor => allDroplets.Contains(neighbor)).ToList();

	externalSurfaceAreaOfAllDroplets += dropletNeighbors.Count();

	IEnumerable<Point3D> neighborsToVisit =
		allNeighbors
			.Except(alreadyVisitedNeighbors)
			.Except(dropletNeighbors);

	foreach (Point3D neighbor in neighborsToVisit)
		positionsToVisit.Add(neighbor);
}

Console.WriteLine("Day 18A");
Console.WriteLine($"Surface area of all scanned droplets: {surfaceAreaOfAllDroplets}");

Console.WriteLine("Day 18B");
Console.WriteLine($"External surface area of all scanned droplets: {externalSurfaceAreaOfAllDroplets}");

bool IsValidPosition(Point3D position) =>
	position.X >= minX && position.X <= maxX &&
	position.Y >= minY && position.Y <= maxY &&
	position.Z >= minZ && position.Z <= maxZ;

IEnumerable<Point3D> GetNeighbors(Point3D position)
{
	yield return position with { X = position.X - 1 }; // Left
	yield return position with { X = position.X + 1 }; // Right
	yield return position with { Y = position.Y - 1 }; // Down
	yield return position with { Y = position.Y + 1 }; // Up
	yield return position with { Z = position.Z - 1 }; // Towards
	yield return position with { Z = position.Z + 1 }; // Away
}

record class Point3D(int X, int Y, int Z);
