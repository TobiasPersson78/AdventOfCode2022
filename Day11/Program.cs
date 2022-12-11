using System.Numerics;
using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

static IEnumerable<T> GetNumbersFromRow<T>(string row) where T: IParsable<T> =>
    Regex
        .Matches(row, @"(\d+)+")
		.Where(match => match.Success)
		.SelectMany(match => match.Groups.Values.Skip(1))
        .Select(value => T.Parse(value.Value, System.Globalization.CultureInfo.InvariantCulture));

static IEnumerable<Monkey<T>> ParseMonkeys<T>(string inputFilename) where T :
	struct,
    IMultiplyOperators<T, T, T>,
    IAdditionOperators<T, T, T>,
    IModulusOperators<T, T, T>,
    IDivisionOperators<T, T, T>,
    IParsable<T>,
	IIncrementOperators<T>
{
	Monkey<T> currentMonkey = new();

    foreach (string row in File.ReadAllLines(inputFilename).Where(item => !string.IsNullOrEmpty(item)))
	{
		if (row.Contains("Monkey "))
            currentMonkey = new();
        else if (row.Contains("Starting items:"))
			currentMonkey.Items = GetNumbersFromRow<T>(row).ToList();
		else if (row.Contains("Operation: new = old * old"))
			currentMonkey.Operation = item => item * item;
		else if (row.Contains("Operation: new = old *"))
		{
			T factor = GetNumbersFromRow<T>(row).First();
			currentMonkey.Operation = item => item * factor;
		}
		else if (row.Contains("Operation: new = old +"))
		{
			T addition = GetNumbersFromRow<T>(row).First();
			currentMonkey.Operation = item => item + addition;
		}
		else if (row.Contains("Test: divisible by"))
			currentMonkey.TestDivisor = GetNumbersFromRow<T>(row).First();
		else if (row.Contains("If true: throw to monkey"))
			currentMonkey.TestTrueTarget = GetNumbersFromRow<int>(row).First();
		else if (row.Contains("If false: throw to monkey"))
		{
			currentMonkey.TestFalseTarget = GetNumbersFromRow<int>(row).First();
			yield return currentMonkey;
		}
    }
}

static void ExecuteRounds<T>(
	IList<Monkey<T>> monkeys,
	int roundsToExecute,
	T worryDivisor,
	T productOfAllDivisors) where T :
    struct,
	IMultiplyOperators<T, T, T>,
    IAdditionOperators<T, T, T>,
    IModulusOperators<T, T, T>,
    IDivisionOperators<T, T, T>,
    IIncrementOperators<T>
{
    for (int i = 0; i < roundsToExecute; ++i)
	{
		foreach (var currentMonkey in monkeys)
		{
			foreach(var currentItem in currentMonkey.Items)
			{
				currentMonkey.InspectionCounter++;
				T newItemWorry = currentMonkey.Operation(currentItem);
				
				newItemWorry /= worryDivisor;
				newItemWorry %= productOfAllDivisors;

				bool resultOfTest = (newItemWorry % currentMonkey.TestDivisor).Equals(default(T));

				int targetMonkey = resultOfTest
					? currentMonkey.TestTrueTarget
					: currentMonkey.TestFalseTarget;

				monkeys[targetMonkey].Items.Add(newItemWorry);
			}
			currentMonkey.Items.Clear();
		}
	}
}

IList<Monkey<int>> monkeysPartA = ParseMonkeys<int>(inputFilename).ToList();
IList<Monkey<BigInteger>> monkeysPartB = ParseMonkeys<BigInteger>(inputFilename).ToList();

int productOfAllTestDivisorsPartA =
    monkeysPartA
        .Select(monkey => monkey.TestDivisor)
        .Aggregate((accumulation, current) => accumulation * current);
BigInteger productOfAllTestDivisorsPartB =
	monkeysPartB
		.Select(monkey => monkey.TestDivisor)
		.Aggregate((accumulation, current) => accumulation * current);

ExecuteRounds(monkeysPartA, 20, 3, productOfAllTestDivisorsPartA);
ExecuteRounds(monkeysPartB, 10_000, 1u, productOfAllTestDivisorsPartB);

int monkeyBusinessLevelPartA =
	monkeysPartA
		.Select(monkey => monkey.InspectionCounter)
		.OrderDescending()
		.Take(2)
		.Aggregate((accumulation, current) => accumulation * current);
BigInteger monkeyBusinessLevelPartB =
    monkeysPartB
        .Select(monkey => monkey.InspectionCounter)
        .OrderDescending()
        .Take(2)
        .Aggregate((accumulation, current) => accumulation * current);

Console.WriteLine("Day 11A");
Console.WriteLine($"Monkey business level: {monkeyBusinessLevelPartA}");

Console.WriteLine("Day 11B");
Console.WriteLine($"Monkey business level: {monkeyBusinessLevelPartB}");

class Monkey<T>	where T:
	struct,
	IMultiplyOperators<T, T, T>,
	IAdditionOperators<T, T, T>,
	IModulusOperators<T, T, T>,
	IDivisionOperators<T, T, T>,
	IIncrementOperators<T>
{
	public T InspectionCounter { get; set; }
	public IList<T> Items { get; set; } = new List<T>();
	public Func<T, T> Operation { get; set; } = item => throw new NotImplementedException();
	public T TestDivisor { get; set; } = default(T);
	public int TestTrueTarget { get; set; }
	public int TestFalseTarget { get; set; }
}