using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

const string BlueprintPattern =
	@"^Blueprint (\d+): Each ore robot costs (\d+) ore. Each clay robot costs (\d+) ore. Each obsidian robot costs (\d+) ore and (\d+) clay. Each geode robot costs (\d+) ore and (\d+) obsidian.$";

IList<Blueprint> blueprintsForPartA =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.Select(item => Regex.Match(item, BlueprintPattern).Groups.Values.Skip(1).Select(item => item.Value).ToList())
		.Select(item => new Blueprint(
			int.Parse(item[0]),
			int.Parse(item[1]),
			int.Parse(item[2]),
			int.Parse(item[3]),
			int.Parse(item[4]),
			int.Parse(item[5]),
			int.Parse(item[6])))
		.ToList();
IList<Blueprint> blueprintsForPartB = blueprintsForPartA.Take(3).ToList();

const int MaxTimeInMinutesPartA = 24;
const int MaxTimeInMinutesPartB = 32;

int sumOfQualityLevels = 0;
foreach (Blueprint blueprint in blueprintsForPartA)
{
	int bestResultFoundForThisBlueprint = 0;
	int numberOfGeodes =
		GetMaxGeodesForBlueprint(
			new RobotsAndMineralsState(0, 1, 0, 0, 0, 0, 0, 0, 0),
			blueprint,
			GetMaxMineralsNeededForBlueprint(blueprint),
			MaxTimeInMinutesPartA,
			new Dictionary<RobotsAndMineralsState, int>(),
			ref bestResultFoundForThisBlueprint);
	Console.WriteLine($"The highest number of geodes found for {blueprint.Id} in {MaxTimeInMinutesPartA} minutes is {numberOfGeodes}.");
	sumOfQualityLevels += blueprint.Id * numberOfGeodes;
}

int productOfGeodeCounts = 1;
foreach (Blueprint blueprint in blueprintsForPartB)
{
	int bestResultFoundForThisBlueprint = 0;
	int numberOfGeodes =
		GetMaxGeodesForBlueprint(
			new RobotsAndMineralsState(0, 1, 0, 0, 0, 0, 0, 0, 0),
			blueprint,
			GetMaxMineralsNeededForBlueprint(blueprint),
			MaxTimeInMinutesPartB,
			new Dictionary<RobotsAndMineralsState, int>(),
			ref bestResultFoundForThisBlueprint);
	Console.WriteLine($"The highest number of geodes found for {blueprint.Id} in {MaxTimeInMinutesPartB} minutes is {numberOfGeodes}.");
	productOfGeodeCounts *= numberOfGeodes;
}

Console.WriteLine("Day 19A");
Console.WriteLine($"Quality levels of all blueprints: {sumOfQualityLevels}");

Console.WriteLine("Day 19B");
Console.WriteLine($"Product of geode count of {blueprintsForPartB.Count} blueprints: {productOfGeodeCounts}");

int GetMaxGeodesForBlueprint(
	RobotsAndMineralsState state,
	Blueprint blueprint,
	MaxMineralsNeeded maxMineralsNeeded,
	int maxTime,
	IDictionary<RobotsAndMineralsState, int> stateAndGeodeCountCache,
	ref int bestResultFoundForThisBlueprint)
{
	if (stateAndGeodeCountCache.TryGetValue(state, out int numberOfGeodes))
		return numberOfGeodes;

	int remainingMinutes = maxTime - state.ElapsedMinutes;
	int maxGeodesFromHereToEndTime =
		remainingMinutes * state.NumberOfGeode + // From existing geode robots
		remainingMinutes * (remainingMinutes + 1) / 2; // If a new geode robot is create every remaining minute

	if (state.NumberOfGeode + maxGeodesFromHereToEndTime < bestResultFoundForThisBlueprint)
	{
		// We can never exceed the best result found. Prune this branch.
		stateAndGeodeCountCache[state] = 0;
		return 0;
	}

	IEnumerable<Action> possibleActions = GetPossibleActionsByPriority(state, blueprint, maxMineralsNeeded);
	RobotsAndMineralsState stateWithMineralsFetched = state with
	{
		ElapsedMinutes = state.ElapsedMinutes + 1,
		NumberOfOre = state.NumberOfOre + state.NumberOfOreRobots,
		NumberOfClay = state.NumberOfClay + state.NumberOfClayRobots,
		NumberOfObsidian = state.NumberOfObsidian + state.NumberOfObsidianRobots,
		NumberOfGeode = state.NumberOfGeode + state.NumberOfGeodeRobots,
	};

	if (stateWithMineralsFetched.ElapsedMinutes == maxTime)
	{
		if (bestResultFoundForThisBlueprint < stateWithMineralsFetched.NumberOfGeode)
			bestResultFoundForThisBlueprint = stateWithMineralsFetched.NumberOfGeode;

		stateAndGeodeCountCache[state] = stateWithMineralsFetched.NumberOfGeode;
		return stateWithMineralsFetched.NumberOfGeode;
	}

	int bestResultFound = 0;

	foreach (Action action in possibleActions)
	{
		var stateAfterRobotFactoryAction =
			action switch
			{
				Action.BuildOreRobot =>
					(stateWithMineralsFetched with
					{
						NumberOfOre = stateWithMineralsFetched.NumberOfOre -
									  blueprint!.OreRobotOreCost,
						NumberOfOreRobots = stateWithMineralsFetched.NumberOfOreRobots + 1

					}),
				Action.BuildClayRobot =>
					(stateWithMineralsFetched with
					{
						NumberOfOre = stateWithMineralsFetched.NumberOfOre -
									  blueprint!.ClayRobotOreCost,
						NumberOfClayRobots = stateWithMineralsFetched.NumberOfClayRobots + 1

					}),
				Action.BuildObsidianRobot =>
					(stateWithMineralsFetched with
					{
						NumberOfOre = stateWithMineralsFetched.NumberOfOre -
									  blueprint!.ObsidianRobotOreCost,
						NumberOfClay = stateWithMineralsFetched.NumberOfClay -
									   blueprint!.ObsidianRobotClayCost,
						NumberOfObsidianRobots = stateWithMineralsFetched.NumberOfObsidianRobots + 1

					}),
				Action.BuildGeodeRobot =>
					(stateWithMineralsFetched with
					{
						NumberOfOre = stateWithMineralsFetched.NumberOfOre -
									  blueprint!.GeodeRobotOreCost,
						NumberOfObsidian = stateWithMineralsFetched.NumberOfObsidian -
										   blueprint!.GeodeRobotObsidianCost,
						NumberOfGeodeRobots = stateWithMineralsFetched.NumberOfGeodeRobots + 1
					}),
				_ => stateWithMineralsFetched,
			};
		int bestResultForThisAction = GetMaxGeodesForBlueprint(
			stateAfterRobotFactoryAction,
			blueprint,
			maxMineralsNeeded,
			maxTime,
			stateAndGeodeCountCache,
			ref bestResultFoundForThisBlueprint);

		bestResultFound = Math.Max(bestResultFound, bestResultForThisAction);
	}

	stateAndGeodeCountCache[state] = bestResultFound;
	return bestResultFound;
}

IEnumerable<Action> GetPossibleActionsByPriority(
	RobotsAndMineralsState state,
	Blueprint blueprint,
	MaxMineralsNeeded maxMineralsNeeded)
{
	// If we can build a geode robot, that should always be done.
	// If we can't build a geode robot, but can build an obsidian robot, that should be done first.
	// If we already produce more minerals of a type than is needed to produce any kind of robot using
	// that mineral, we don't need to produce more robots producing that mineral.

	if (state.NumberOfOre >= blueprint.GeodeRobotOreCost &&
		state.NumberOfObsidian >= blueprint.GeodeRobotObsidianCost)
	{
		yield return Action.BuildGeodeRobot;
		yield break;
	}

	if (state.NumberOfObsidianRobots < maxMineralsNeeded.MaxObsidianNeeded &&
		state.NumberOfOre >= blueprint.ObsidianRobotOreCost &&
		state.NumberOfClay >= blueprint.ObsidianRobotClayCost)
	{
		yield return Action.BuildObsidianRobot;
	}

	if (state.NumberOfClayRobots < maxMineralsNeeded.MaxClayNeeded &&
		state.NumberOfOre >= blueprint.ClayRobotOreCost)
	{
		yield return Action.BuildClayRobot;
	}

	if (state.NumberOfOreRobots < maxMineralsNeeded.MaxOreNeeded &&
		state.NumberOfOre >= blueprint.OreRobotOreCost)
	{
		yield return Action.BuildOreRobot;
	}

	yield return Action.BuildNothing;
}

MaxMineralsNeeded GetMaxMineralsNeededForBlueprint(Blueprint blueprint) =>
	new(
		new[]
		{
			blueprint.OreRobotOreCost,
			blueprint.ClayRobotOreCost,
			blueprint.ObsidianRobotOreCost,
			blueprint.GeodeRobotOreCost,
		}.Max(),
		blueprint.ObsidianRobotClayCost,
		blueprint.GeodeRobotObsidianCost);

record MaxMineralsNeeded(
	int MaxOreNeeded,
	int MaxClayNeeded,
	int MaxObsidianNeeded);

enum Action
{
	BuildNothing,
	BuildOreRobot,
	BuildClayRobot,
	BuildObsidianRobot,
	BuildGeodeRobot,
}

record Blueprint(
	int Id,
	int OreRobotOreCost,
	int ClayRobotOreCost,
	int ObsidianRobotOreCost,
	int ObsidianRobotClayCost,
	int GeodeRobotOreCost,
	int GeodeRobotObsidianCost);

record RobotsAndMineralsState(
	int ElapsedMinutes,
	int NumberOfOreRobots,
	int NumberOfClayRobots,
	int NumberOfObsidianRobots,
	int NumberOfGeodeRobots,
	int NumberOfOre,
	int NumberOfClay,
	int NumberOfObsidian,
	int NumberOfGeode);
