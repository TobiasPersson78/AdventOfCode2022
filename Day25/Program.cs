using System.Collections.Immutable;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

IList<char> snafuDigitsInOrder = new[]
{
	'=',
	'-',
	'0',
	'1',
	'2',
};
IDictionary<char, int> snafuDigitToIntegerValues = new Dictionary<char, int>
{
	{ '2', 2 },
	{ '1', 1 },
	{ '0', 0 },
	{ '-', -1 },
	{ '=', -2 },
};

long sumOfSnafuNumbers =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.Sum(SnafuToDecimal);

string snafuNumber = DecimalToSnafu(sumOfSnafuNumbers);

Console.WriteLine("Day 25");
Console.WriteLine($"The SNAFU number is {snafuNumber}.");

long SnafuToDecimal(string snafuNumber) =>
	snafuNumber.Aggregate(0L, (sum, snafuDigit) => sum *5 + snafuDigitToIntegerValues[snafuDigit]);

string DecimalToSnafu(long number) =>
	number == 0
		? string.Empty
		: DecimalToSnafu((number + 2) / 5) + snafuDigitsInOrder[Convert.ToInt32((number + 2) % 5)];
