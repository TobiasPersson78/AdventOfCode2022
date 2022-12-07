bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

char[] messageStream = File.ReadAllText(inputFilename).Trim().ToCharArray();

const int slidingWindowSizePartA = 4;
const int slidingWindowSizePartB = 14;

int FindFirstAllUniqueWindowIndex(char[] streamContent, int slidingWindowSize)
{
	for (int index = slidingWindowSize; index < messageStream.Length; ++index)
	{
		// As the end index is exclusive, the returned index will coincidentally be the same as
		// the wanted one-based indexing would yield.
		if (streamContent[(index - slidingWindowSize)..index].Distinct().Count() == slidingWindowSize)
		{
			return index;
		}
	}

	throw new InvalidOperationException("No all unique window found.");
}

int startOfPacketIndex = FindFirstAllUniqueWindowIndex(messageStream, slidingWindowSizePartA);
int startOfMessageIndex = FindFirstAllUniqueWindowIndex(messageStream, slidingWindowSizePartB);

Console.WriteLine("Day 6A");
Console.WriteLine($"First packet index: {startOfPacketIndex}");

Console.WriteLine("Day 6B");
Console.WriteLine($"First message index: {startOfMessageIndex}");
