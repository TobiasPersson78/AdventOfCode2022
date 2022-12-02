bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

var pointsForShape = new Dictionary<Shape, int>
{
	{ Shape.Rock, 1 },
	{ Shape.Paper, 2 },
	{ Shape.Scissor, 3 },
};

var pointsForResult = new Dictionary<Result, int>
{
	{ Result.Loss, 0 },
	{ Result.Draw, 3 },
	{ Result.Win, 6 },
};

var shapeForChar = new Dictionary<char, Shape>
{
	{ 'A', Shape.Rock },
	{ 'B', Shape.Paper },
	{ 'C', Shape.Scissor },
	{ 'X', Shape.Rock },
	{ 'Y', Shape.Paper },
	{ 'Z', Shape.Scissor },
};

var resultForShapes = new Dictionary<(Shape Opponent, Shape Myself), Result>
{
	{ (Shape.Rock, Shape.Rock), Result.Draw },
	{ (Shape.Rock, Shape.Paper), Result.Win },
	{ (Shape.Rock, Shape.Scissor), Result.Loss },
	{ (Shape.Paper, Shape.Rock), Result.Loss },
	{ (Shape.Paper, Shape.Paper), Result.Draw },
	{ (Shape.Paper, Shape.Scissor), Result.Win },
	{ (Shape.Scissor, Shape.Rock), Result.Win },
	{ (Shape.Scissor, Shape.Paper), Result.Loss },
	{ (Shape.Scissor, Shape.Scissor), Result.Draw },
};

var shapeForOpponentsShapeAndDesiredResult = new Dictionary<(Shape Opponent, char DesiredResult), Shape>
{
	{ (Shape.Rock, 'X'), Shape.Scissor },
	{ (Shape.Rock, 'Y'), Shape.Rock },
	{ (Shape.Rock, 'Z'), Shape.Paper },
	{ (Shape.Paper, 'X'), Shape.Rock },
	{ (Shape.Paper, 'Y'), Shape.Paper },
	{ (Shape.Paper, 'Z'), Shape.Scissor },
	{ (Shape.Scissor, 'X'), Shape.Paper },
	{ (Shape.Scissor, 'Y'), Shape.Scissor },
	{ (Shape.Scissor, 'Z'), Shape.Rock },
};

int totalScorePartA = 0;
int totalScorePartB = 0;

foreach (string currentRow in File.ReadAllLines(inputFilename).Where(item => !string.IsNullOrEmpty(item)))
{
	var opponentsShape = shapeForChar[currentRow.First()];
	var myShapePartA = shapeForChar[currentRow.Last()];
	var myShapePartB = shapeForOpponentsShapeAndDesiredResult[(opponentsShape, currentRow.Last())];

	totalScorePartA += pointsForShape[myShapePartA];
	totalScorePartB += pointsForShape[myShapePartB];

	var resultPartA = resultForShapes[(opponentsShape, myShapePartA)];
	var resultPartB = resultForShapes[(opponentsShape, myShapePartB)];

	totalScorePartA += pointsForResult[resultPartA];
	totalScorePartB += pointsForResult[resultPartB];
}

Console.WriteLine("Day 2A");
Console.WriteLine($"Total score from following original strategy guide: {totalScorePartA} points.");

Console.WriteLine("Day 2B");
Console.WriteLine($"Total score from following adjusted strategy guide: {totalScorePartB} points.");

enum Shape
{
	Rock,
	Paper,
	Scissor
}

enum Result
{
	Loss,
	Draw,
	Win
}