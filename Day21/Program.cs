using System.Collections.Immutable;
using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

IDictionary<string, (Func<long> Function, string[] Dependencies)>? monkeyLookup = null;
monkeyLookup =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.Select(item =>
		{
			Match matchForNumber = Regex.Match(item, @"^([a-z]{4}): (\d+$)");
			if (matchForNumber.Success)
			{
				string name = matchForNumber.Groups[1].Value;
				long number = long.Parse(matchForNumber.Groups[2].Value);

				Func<long> function = () => number;

				return (Name: name, Function: function, Dependencies: Array.Empty<string>());
			}

			Match matchForOperation = Regex.Match(item, @"^([a-z]{4}): ([a-z]{4}) ([+\-*/]) ([a-z]{4})$");
			if (matchForOperation.Success)
			{
				string name = matchForOperation.Groups[1].Value;
				string leftOperand = matchForOperation.Groups[2].Value;
				char operation = matchForOperation.Groups[3].Value.First();
				string rightOperand = matchForOperation.Groups[4].Value;

				Func<long> function = operation switch
				{
					'+' => () => monkeyLookup![leftOperand].Function() + monkeyLookup![rightOperand].Function(),
					'-' => () => monkeyLookup![leftOperand].Function() - monkeyLookup![rightOperand].Function(),
					'*' => () => monkeyLookup![leftOperand].Function() * monkeyLookup![rightOperand].Function(),
					'/' => () => monkeyLookup![leftOperand].Function() / monkeyLookup![rightOperand].Function(),
					_ => throw new IOException($"Unexpected math operation: '{item}'."),
				};

				return (Name: name, Function: function, Dependencies: new string[] { leftOperand, rightOperand });
			}

			throw new IOException($"Unexpected row content: '{item}'.");
		})
		.ToDictionary(item => item.Name, item => (item.Function, item.Dependencies));

long rootYells = monkeyLookup["root"].Function();

const string human = "humn";

IList<string> rootChildren = monkeyLookup["root"].Dependencies;

if (rootChildren?.Count != 2)
	throw new InvalidOperationException("Expected two children.");

Func<long> evaluateFunction;
long targetValue;

if (GetDependencies(rootChildren.First(), monkeyLookup).Contains(human))
{
	evaluateFunction = monkeyLookup[rootChildren.First()].Function;
	targetValue = monkeyLookup[rootChildren.Last()]!.Function();
}
else
{
	evaluateFunction = monkeyLookup[rootChildren.Last()].Function;
	targetValue = monkeyLookup[rootChildren.First()]!.Function();
}

long leftBorder = int.MinValue * 100_000L;
SetHumanValue(leftBorder);
long leftValue = evaluateFunction() - targetValue;

long rightBorder = int.MaxValue * 100_000L;
SetHumanValue(rightBorder);
long rightValue = evaluateFunction() - targetValue;
long humanValue;

if (Math.Sign(leftValue) == Math.Sign(rightValue))
	throw new InvalidOperationException("Expected borders to have different signs.");

while (true)
{
	SetHumanValue((leftBorder + rightBorder) / 2);
	long currentResult = evaluateFunction() - targetValue;

	if (currentResult == 0)
		break;

	if (Math.Sign(leftValue) == Math.Sign(currentResult))
	{
		leftBorder = humanValue;
		leftValue = currentResult;
	}
	else
	{
		rightBorder = humanValue;
	}
}

Console.WriteLine("Day 21A");
Console.WriteLine($"Value the root monkey yells: {rootYells}.");

Console.WriteLine("Day 21B");
Console.WriteLine($"Value the human yells: {humanValue}.");

void SetHumanValue(long value)
{
	humanValue = value;
	monkeyLookup![human] = (() => value, Array.Empty<string>());
}

IImmutableSet<string> GetDependencies(
	string name,
	IDictionary<string, (Func<long> Function, string[] Dependencies)> itemsByName)
{
	IImmutableSet<string> dependencies = Enumerable.Repeat(name, 1).ToImmutableHashSet();

	foreach (string dependencyName in itemsByName[name].Dependencies)
	{
		dependencies = dependencies.Union(GetDependencies(dependencyName, itemsByName));
	}

	return dependencies;
}
