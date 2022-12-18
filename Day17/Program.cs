bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

const char moveLeft = '<';
const char moveRight = '>';

IList<char> leftRightMovementPattern = File.ReadAllText(inputFilename).TrimEnd().ToCharArray();

int[] blockMinus = new[]
{
	0b111100,
};
int[] blockPlus = new []
{
	0b010000,
	0b111000,
	0b010000,
};
int[] blockJ = new[]
{
	0b001000,
	0b001000,
	0b111000,
};
int[] blockI = new[]
{
	0b100000,
	0b100000,
	0b100000,
	0b100000,
};
int[] blockO = new[]
{
	0b110000,
	0b110000,
};
IList<int[]> allBlocksInFallOrder = new[]
{
	blockMinus,
	blockPlus,
	blockJ,
	blockI,
	blockO
};
int maxHeightOfOneBlockSequence = allBlocksInFallOrder.Sum(block => block.Length);

const int bottom = 0b111111111;
const int walls = 0b100000001;
const int emptyLinesBeforeNewBlock = 3;
const int numberOfFallingRocksPartA = 2022;
const long numberOfFallingRocksPartB = 1_000_000_000_000;

long heightOfRocksPartA = CalculateHeight(numberOfFallingRocksPartA);
long heightOfRocksPartB = CalculateHeight(numberOfFallingRocksPartB);

Console.WriteLine("Day 17A");
Console.WriteLine($"Height of rock tower after {numberOfFallingRocksPartA} rocks: {heightOfRocksPartA}");

Console.WriteLine("Day 17B");
Console.WriteLine($"Height of rock tower after {numberOfFallingRocksPartB} rocks: {heightOfRocksPartB}");

long CalculateHeight(long numberOfFallingRocks)
{
	List<int> tower = new() { bottom };
	int rockIndex = 0;
	int leftRightIndex = 0;

	Dictionary<IndexAndTowerState, (long FallingRockIndex, int TowerHeight)> cycleStateLookup = new();
	long cycleAdjustmentForTowerCount = 0;

	for (long fallingRockIndex = 0; fallingRockIndex < numberOfFallingRocks; fallingRockIndex++)
	{
		int[] block = allBlocksInFallOrder[rockIndex].ToArray();

		// Cache the index and tower states.
		if (cycleAdjustmentForTowerCount == 0 && tower.Count + 1 > maxHeightOfOneBlockSequence)
		{
			IndexAndTowerState newState = new IndexAndTowerState
			{
				RockIndex = rockIndex,
				MovementIndex = leftRightIndex,
				ToTowerRow = tower.Count - 1,
				FromTowerRow = tower.Count - maxHeightOfOneBlockSequence,
				Tower = tower,
			};

			if (cycleStateLookup.TryGetValue(newState, out (long FallingRockIndex, int TowerHeight) previousIndexAndHeight))
			{
				long cycleLength = fallingRockIndex - previousIndexAndHeight.FallingRockIndex;
				int cycleHeight = tower.Count - previousIndexAndHeight.TowerHeight;
				Console.WriteLine($"Cache hit: Heights {previousIndexAndHeight.TowerHeight} and {tower.Count} are cyclic, cycle length {cycleLength}, after {fallingRockIndex} rocks.");
				long remainingRocks = numberOfFallingRocks - fallingRockIndex;
				long numberOfRemainingCycles = remainingRocks / cycleLength;
				cycleAdjustmentForTowerCount = numberOfRemainingCycles * cycleHeight;
				fallingRockIndex += numberOfRemainingCycles * cycleLength;
			}
			else
			{
				// Save the current state
				cycleStateLookup[newState] = (fallingRockIndex, tower.Count);
			}
		}

		// Add empty space.
		tower.AddRange(Enumerable.Repeat(walls, emptyLinesBeforeNewBlock + block.Length));
		int blockTopLine = tower.Count - 1;

		bool couldMoveDown = false;
		do
		{
			char horizontalMovement = leftRightMovementPattern[leftRightIndex];

			// Move the rock left or right.
			TryMoveHorizontally(horizontalMovement, blockTopLine, block, tower);

			// Move the rock down.
			couldMoveDown = TryMoveDown(blockTopLine, block, tower);
			--blockTopLine;

			// Increase the horizontal movement index.
			++leftRightIndex;
			leftRightIndex %= leftRightMovementPattern.Count;
		}
		while (couldMoveDown);

		// Remove only walls from top of tower.
		while (tower[tower.Count - 1] == walls)
			tower.RemoveAt(tower.Count - 1);

		// Increase the rock index.
		++rockIndex;
		rockIndex %= allBlocksInFallOrder.Count;
	}

	return cycleAdjustmentForTowerCount + tower.Count -1; // -1: Do not include the bottom.
}

bool TryMoveDown(int blockTopLine, int[] block, IList<int> tower)
{
	bool couldMoveDown = true;
	for (int i = block.Length - 1; couldMoveDown && i >= 0; --i)
	{
		if ((tower[blockTopLine - i - 1] & block[i]) != 0)
			couldMoveDown = false;
	}

	if (!couldMoveDown)
	{
		for (int i = block.Length - 1; i >= 0; --i)
		{
			tower[blockTopLine - i] |= block[i];
		}
	}

	return couldMoveDown;
}

void TryMoveHorizontally(char direction, int blockTopLine, int[] block, IList<int> tower)
{
	if (direction != moveLeft && direction != moveRight)
		throw new ArgumentException($"Unexpected direction {direction}.");

	var movedBlock =
		direction == moveLeft
			? block.Select(item => item << 1).ToArray()
			: block.Select(item => item >> 1).ToArray();

	for(int i = movedBlock.Length - 1; i >= 0; --i )
	{
		if ((tower[blockTopLine - i] & movedBlock[i]) != 0)
			return;
	}

	Array.Copy(movedBlock, block, block.Length);
}

class IndexAndTowerState
{
	public int RockIndex { get; init; }
	public int MovementIndex { get; init; }
	public int FromTowerRow { get; init; }
	public int ToTowerRow { get; init; }
	public IList<int>? Tower { get; init; }

	public override bool Equals(object? obj)
	{
		return Equals(obj as IndexAndTowerState);
	}

	public bool Equals(IndexAndTowerState? other)
	{
		if (other is null)
			return false;

		if( RockIndex != other.RockIndex ||
			MovementIndex != other.MovementIndex ||
			ToTowerRow - FromTowerRow != other.ToTowerRow - other.FromTowerRow ||
			Tower != other.Tower)
		{
			return false;
		}

		for( int i = 0; i <= ToTowerRow - FromTowerRow; ++i )
		{
			if (Tower![FromTowerRow + i] != Tower![other.FromTowerRow + i])
				return false;
		}

		return true;
	}

	public override int GetHashCode()
	{
		HashCode hashCode = new HashCode();

		hashCode.Add(RockIndex);
		hashCode.Add(MovementIndex);

		for (int i = 0; i <= ToTowerRow - FromTowerRow; ++i)
		{
			hashCode.Add(Tower![FromTowerRow + i]);
		}

		return hashCode.ToHashCode();
	}
}
