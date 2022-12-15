using System.Diagnostics;
using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

IList<SensorAndBeacon> listOfSensorAndBeacon =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.Select(row =>
		{
			const string sensorAndBeaconPattern = @"^Sensor at x=(-?\d+), y=(-?\d+): closest beacon is at x=(-?\d+), y=(-?\d+)$";

			var match = Regex.Match(row, sensorAndBeaconPattern);

			if (!match.Success)
				throw new IOException($"Failed to parse {row}.");

			(long X, long Y) sensorPosition = (long.Parse(match.Groups[1].Value), long.Parse(match.Groups[2].Value));
			(long X, long Y) beaconPosition = (long.Parse(match.Groups[3].Value), long.Parse(match.Groups[4].Value));

			return new SensorAndBeacon
			{
				SensorPosition = sensorPosition,
				BeaconPosition = beaconPosition,
				ManhattanDistance = CalculateManhattanDistance(sensorPosition, beaconPosition)
			};
		})
		.ToList();
var beaconAndSensorPositions =
	listOfSensorAndBeacon
		.SelectMany(item => new[] { item.SensorPosition, item.BeaconPosition })
		.ToList();

long xMin = listOfSensorAndBeacon.Min(item => Math.Min(item.SensorPosition.X, item.BeaconPosition.X));
long xMax = listOfSensorAndBeacon.Max(item => Math.Max(item.SensorPosition.X, item.BeaconPosition.X));
long width = xMax - xMin + 1;
long yMin = listOfSensorAndBeacon.Min(item => Math.Min(item.SensorPosition.Y, item.BeaconPosition.Y));
long yMax = listOfSensorAndBeacon.Max(item => Math.Max(item.SensorPosition.Y, item.BeaconPosition.Y));
long height = yMax - yMin + 1;

Console.WriteLine($"Grid: From x={xMin} to x={xMax} (width {width}).");
Console.WriteLine($"Grid: From y={yMin} to y={yMax} (height {height}).");

long yRowPartA = useExampleInput
	? 10
	: 2000000;

int numberOfLocationsThatCannotContainBeacon =
	Enumerable
		.Range((int)xMin, (int)width)
		.Count(xPosition =>
			listOfSensorAndBeacon
				.Any(sensorAndBeacon =>
					!beaconAndSensorPositions.Contains((xPosition, yRowPartA)) &&
					CalculateManhattanDistance((xPosition, yRowPartA), sensorAndBeacon.SensorPosition) <= sensorAndBeacon.ManhattanDistance));

long searchXStart = 0;
long searchYStart = 0;
long searchXEnd = useExampleInput
	? 20
	: 4_000_000;
long searchYEnd = useExampleInput
	? 20
	: 4_000_000;

List<SensorAndBeacon> listOfSensorAndBeaconSortedByX =
	listOfSensorAndBeacon
		.OrderBy(item => item.SensorPosition.X)
		.ToList();

List<(long X, long Y)> positionsOutsideDiamonds =
	listOfSensorAndBeacon
		.SelectMany(item => item.OutsideDiamond())
		.Where(item =>
			item.X >= searchXStart && item.X <= searchXEnd &&
			item.Y >= searchYStart && item.Y <= searchYEnd)
		.ToList();

long tuningFrequency = -1;
Stopwatch stopwatchParallel = Stopwatch.StartNew();
Parallel.ForEach(
	positionsOutsideDiamonds,
	positionToTest =>
	{
		if (tuningFrequency < 0 && !listOfSensorAndBeacon.Any(sensorAndBeacon => IsInsideDiamond(positionToTest, sensorAndBeacon)))
		{
			Console.WriteLine($"Position x={positionToTest.X}, y={positionToTest.Y} is not covered.");
			tuningFrequency = positionToTest.X * 4_000_000 + positionToTest.Y;
		}
	});
stopwatchParallel.Stop();
Console.WriteLine($"Parallel execution took {stopwatchParallel.Elapsed}.");

tuningFrequency = -1;
Stopwatch stopwatchForeach = Stopwatch.StartNew();
foreach (var positionToTest in positionsOutsideDiamonds)
{
	if (tuningFrequency < 0 && !listOfSensorAndBeacon.Any(sensorAndBeacon => IsInsideDiamond(positionToTest, sensorAndBeacon)))
	{
		Console.WriteLine($"Position x={positionToTest.X}, y={positionToTest.Y} is not covered.");
		tuningFrequency = positionToTest.X * 4_000_000 + positionToTest.Y;
		break;
	}
}
stopwatchForeach.Stop();
Console.WriteLine($"Foreach execution took {stopwatchForeach.Elapsed}.");

Console.WriteLine("Day 15A");
Console.WriteLine($"Number possible locations: {numberOfLocationsThatCannotContainBeacon}");

Console.WriteLine("Day 15B");
Console.WriteLine($"Tuning frequency: {tuningFrequency}");

long CalculateManhattanDistance((long X, long Y) firstPosition, (long X, long Y) secondPosition)
{
	return Math.Abs(firstPosition.X - secondPosition.X) + Math.Abs(firstPosition.Y - secondPosition.Y);
}

bool IsInsideDiamond((long X, long Y) position, SensorAndBeacon sensorAndBeacon) =>
	CalculateManhattanDistance(position, sensorAndBeacon.SensorPosition) <= sensorAndBeacon.ManhattanDistance;

public class SensorAndBeacon
{
	public (long X, long Y) SensorPosition { get; init; }

	public (long X, long Y) BeaconPosition { get; init; }

	public long ManhattanDistance { get; init; }

	public override string ToString() =>
		$"Sensor at x={SensorPosition.X}, y={SensorPosition.Y}: closest beacon is at x={BeaconPosition.X}, y={BeaconPosition.Y}";

	public IEnumerable<(long X, long Y)> OutsideDiamond()
	{
		long yOffSet = 0;
		for (long x = SensorPosition.X - ManhattanDistance - 1; x <= SensorPosition.X; x++)
		{
			yield return (x, SensorPosition.Y + yOffSet);
			if (yOffSet != 0)
				yield return (x, SensorPosition.Y - yOffSet);

			yOffSet++;
		}

		for (long x = SensorPosition.X + 1; x <= SensorPosition.X + ManhattanDistance + 1; x++)
		{
			yOffSet--;

			yield return (x, SensorPosition.Y + yOffSet);
			if (yOffSet != 0)
				yield return (x, SensorPosition.Y - yOffSet);
		}
	}
}
