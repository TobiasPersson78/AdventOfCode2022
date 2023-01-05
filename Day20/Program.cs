using System.Collections.Generic;

bool useExampleInput = true;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

IList<LongWrapper> originalOrder =
	File
		.ReadAllLines(inputFilename)
		.Where(item => !string.IsNullOrEmpty(item))
		.Select(item => new LongWrapper { Value = int.Parse(item) })
		.ToList();
const long DecryptionKeyPartA = 1;
const long DecryptionKeyPartB = 811589153;
const int NumberOfMixingsPartA = 1;
const int NumberOfMixingsPartB = 10;
long coordinatePartA = GetCoordinate(originalOrder, DecryptionKeyPartA, NumberOfMixingsPartA);
long coordinatePartB = GetCoordinate(originalOrder, DecryptionKeyPartB, NumberOfMixingsPartB);

Console.WriteLine("Day 20A");
Console.WriteLine($"Grove coordinate: {coordinatePartA}.");

Console.WriteLine("Day 20B");
Console.WriteLine($"Grove coordinate: {coordinatePartB}.");

long GetCoordinate(IList<LongWrapper> originalOrder, long decryptionKey, int numberOfMixings)
{
	IList<LongWrapper> scaledOriginalOrder =
		originalOrder
			.Select(item => new LongWrapper { Value = item.Value * decryptionKey })
			.ToList();

	IList<LongWrapper> mixedOrder = DoMixing(scaledOriginalOrder, numberOfMixings);
	int indexOfZero = mixedOrder.IndexOf(mixedOrder.First(item => item.Value == 0));
	int[] relativeIndicesForCoordinates = new[] { 1000, 2000, 3000 };
	IList<long> coordinateParts =
		relativeIndicesForCoordinates
			.Select(item => mixedOrder[(indexOfZero + item) % originalOrder.Count].Value)
			.ToList();
	long coordinate = coordinateParts.Sum();
	return coordinate;
}

IList<LongWrapper> DoMixing(IList<LongWrapper> originalOrder, int numberOfMixings)
{
	IList<LongWrapper> mixedOrder = originalOrder.ToList();

	for (int i = 0; i < numberOfMixings; ++i)
	{
		foreach (LongWrapper currentItem in originalOrder)
		{
			int currentIndex = mixedOrder.IndexOf(currentItem);
			long desiredChange = currentItem.Value;

			// Due to circularity, being first and being last is the same position, so normal
			// modulus calculations does not work without adjustment.
			int circularLength = mixedOrder.Count - 1;
			desiredChange %= circularLength;

			if (currentIndex + desiredChange < 0)
			{
				desiredChange += circularLength;
			}

			if (currentIndex + desiredChange >= mixedOrder.Count)
			{
				desiredChange -= circularLength;
			}

			int newIndex = Convert.ToInt32(currentIndex + desiredChange);

			mixedOrder.RemoveAt(currentIndex);
			mixedOrder.Insert(newIndex, currentItem);
		}
	}

	return mixedOrder;
}

class LongWrapper
{
	public long Value { get; set; }
}
