bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

string messageStream = File.ReadAllText(inputFilename).Trim();

IList<char> slidingWindowBufferPartA = new List<char>();
IList<char> slidingWindowBufferPartB = new List<char>();
int startOfPacketIndex = -1;
int startOfMessageIndex = -1;
const int slidingWindowSizePartA = 4;
const int slidingWindowSizePartB = 14;

for (int index = 0; index < messageStream.Length; ++index)
{
	if (slidingWindowBufferPartA.Count == slidingWindowSizePartA)
		slidingWindowBufferPartA.RemoveAt(0);
	if (slidingWindowBufferPartB.Count == slidingWindowSizePartB)
		slidingWindowBufferPartB.RemoveAt(0);

	slidingWindowBufferPartA.Add(messageStream[index]);
	slidingWindowBufferPartB.Add(messageStream[index]);

	if (slidingWindowBufferPartA.Distinct().Count() == slidingWindowSizePartA &&
		startOfPacketIndex < 0)
	{
		startOfPacketIndex = index + 1; // Adjust for one-based indexing.
	}
	if (slidingWindowBufferPartB.Distinct().Count() == slidingWindowSizePartB &&
		startOfMessageIndex < 0)
	{
		startOfMessageIndex = index + 1; // Adjust for one-based indexing.
	}
}


Console.WriteLine("Day 6A");
Console.WriteLine($"First packet index: {startOfPacketIndex}");

Console.WriteLine("Day 6B");
Console.WriteLine($"First message index: {startOfMessageIndex}");
