using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

const string valveParsePattern = @"([A-Z]{2}|\d+)";
const string startPositionId = "AA";
const int infinitePathTime = int.MaxValue / 2 - 1; // Large enough to not overflow when added to the same value.
const int neightborPathTravelTime = 1;
const int timeToOpenValve = 1;
const int timeToTrainElephant = 4;
const int maxTimeToReleasePreassure = 30;

IList<Valve> allValves =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.Select(row =>
			Regex
				.Matches(row, valveParsePattern)
				.Select(match => match.Value)
				.ToList())
		.Select(matchingStrings =>
			new Valve(
				matchingStrings.First(),
				int.Parse(matchingStrings.Skip(1).First()),
				matchingStrings.Skip(2).ToList()))
		.ToList();
IDictionary<Valve, Dictionary<Valve, int>> allPathCosts =
	allValves
		.ToDictionary(
			fromValve => fromValve,
			fromValve =>
				allValves
					.ToDictionary(
						toValve => toValve,
						toValve => fromValve.Neighbors.Any(neightborId => neightborId == toValve.ID)
							? neightborPathTravelTime
							: infinitePathTime));
UpdateShortestPathUsingFloydWarshall(allPathCosts);
RemoveZeroFlowValvesAsTargets(allPathCosts);

Valve startPositionValve = allValves.First(item => item.ID == startPositionId);

int maximumReleasedPressurePartA = 0;
DepthFirstSearch(
	allPathCosts,
	0,
	startPositionValve,
	null,
	new List<(Valve Valve, int Time)>(),
	0,
	maxTimeToReleasePreassure,
	ref maximumReleasedPressurePartA);

int maximumReleasedPressurePartB = 0;
DepthFirstSearch(
	allPathCosts,
	0,
	startPositionValve,
	startPositionValve,
	new List<(Valve Valve, int Time)>(),
	0,
	maxTimeToReleasePreassure - timeToTrainElephant,
	ref maximumReleasedPressurePartB);

Console.WriteLine("Day 16A");
Console.WriteLine($"Maximum released pressaure: {maximumReleasedPressurePartA}");

Console.WriteLine("Day 16B");
Console.WriteLine($"Maximum released pressaure: {maximumReleasedPressurePartB}");

void DepthFirstSearch(
	IDictionary<Valve, Dictionary<Valve, int>> allPathCosts,
	int currentReleasedPreassure,
	Valve curentValve,
	Valve? secondReleaserOfValvesStartPosition,
	IList<(Valve Valve, int Time)> visistedValvesAndTimes,
	int currentTime,
	int maxTime,
	ref int maximumReleasedPressure)
{
	string path = string.Join(",", visistedValvesAndTimes.Select(item => $"{item.Valve.ID} ({item.Time})"));

	if (currentReleasedPreassure > maximumReleasedPressure)
	{
		maximumReleasedPressure = currentReleasedPreassure;
		Console.WriteLine($"New best path: {maximumReleasedPressure} total released pressure, path {path}.");
	}

	foreach (Valve toValve in allPathCosts[curentValve].Keys)
	{
		int toValveTravelTime = allPathCosts[curentValve][toValve];
		int timeToMoveToAndOpenToValve = toValveTravelTime + timeToOpenValve;
		int timeAfterMovingAndOpeningToValve = currentTime + timeToMoveToAndOpenToValve;

		if (!visistedValvesAndTimes.Any(item => item.Valve == toValve) &&
			timeAfterMovingAndOpeningToValve < maxTime)
		{
			int releasedPreassureFromToValve = (maxTime - timeAfterMovingAndOpeningToValve) * toValve.FlowSpeed;

			DepthFirstSearch(
				allPathCosts,
				currentReleasedPreassure + releasedPreassureFromToValve,
				toValve,
				secondReleaserOfValvesStartPosition,
				visistedValvesAndTimes.Concat(new[] { (toValve, currentTime + timeToMoveToAndOpenToValve) }).ToList(),
				timeAfterMovingAndOpeningToValve,
				maxTime,
				ref maximumReleasedPressure);
		}
	}

	if (secondReleaserOfValvesStartPosition != null)
		DepthFirstSearch(
			allPathCosts,
			currentReleasedPreassure,
			secondReleaserOfValvesStartPosition,
			null,
			visistedValvesAndTimes,
			0,
			maxTime,
			ref maximumReleasedPressure);
}

void RemoveZeroFlowValvesAsTargets(IDictionary<Valve, Dictionary<Valve, int>> allPathCosts)
{
	IList<Valve> allZeroFlowValves =
		allPathCosts
			.Keys
			.Where(item => item.FlowSpeed == 0)
			.ToList();

	foreach (Valve fromValve in allPathCosts.Keys)
	{
		var currentToValveLookup = allPathCosts[fromValve];

		foreach (Valve zeroFlowValve in allZeroFlowValves)
		{
			currentToValveLookup.Remove(zeroFlowValve);
		}
	}
}


void UpdateShortestPathUsingFloydWarshall(IDictionary<Valve, Dictionary<Valve, int>> allPathCosts)
{
	foreach (Valve kValve in allPathCosts.Keys)
	{
		foreach (Valve iValve in allPathCosts.Keys)
		{
			foreach (Valve jValve in allPathCosts.Keys)
			{
				int ikCost = allPathCosts[iValve][kValve];
				int kjCost = allPathCosts[kValve][jValve];
				int ijCost = allPathCosts[iValve][jValve];

				if (ikCost + kjCost < ijCost)
					allPathCosts[iValve][jValve] = ikCost + kjCost;
			}
		}
	}
}

public class Valve
{
	public string ID { get; }

	public int FlowSpeed { get; }

	public IList<string> Neighbors { get; }

	public Valve(string id, int flowSpeed, IList<string> neighbors)
	{
		ID = id;
		FlowSpeed = flowSpeed;
		Neighbors = neighbors;
	}

	public override string ToString() =>
		$"Valve {ID}, flow speed {FlowSpeed}, neighbors {string.Join(", ", Neighbors)}.";
}
