using System.Text.RegularExpressions;

bool useExampleInput = false;

string inputFilename = useExampleInput
	? "exampleInput.txt"
	: "input.txt";

Node<FolderData> rootFolder = new(null);
rootFolder.Value.Name = "/";

Node<FolderData> currentFolder = rootFolder;

foreach (string row in File.ReadAllLines(inputFilename).Where(item => !string.IsNullOrEmpty(item)))
{
	if (TryParseAsGoToRoot(row))
	{
		currentFolder = rootFolder;
		continue;
	}

	if (TryParseAsGoToParent(row))
	{
		currentFolder = currentFolder.Parent!;
		continue;
	}

	var (isGoToFolderCommand, targetFolder) = TryParseAsGoToFolderCommand(row);
	if (isGoToFolderCommand)
	{
		currentFolder = currentFolder.Children.First(folderNode => folderNode.Value.Name == targetFolder);
		continue;
	}

	if (TryParseAsListCommand(row))
		continue;

	var (isFolder, folderName) = TryParseAsListResponseFolder(row);
	if (isFolder)
	{
		var childFolder = currentFolder.AddChild();
		childFolder.Value.Name = folderName;
		continue;
	}

	var (isFile, filename, fileSize) = TryParseAsListResponseFile(row);
	if (isFile)
	{
		currentFolder.Value.Files.Add(new FileData() { Name = filename, Size = fileSize });
		continue;
	}

	throw new InvalidOperationException($"Unexpected row content: '{row}'");
}

CalculateFolderSize(rootFolder);

const int FolderSizeLimit = 100000;
IList<Node<FolderData>> selectedFolders = new List<Node<FolderData>>();

rootFolder.Visit(folderNode =>
{
	if (folderNode.Value.LastCalculatedSize < FolderSizeLimit)
		selectedFolders.Add(folderNode);
});

int sumOfSmallFolderSizes = selectedFolders.Sum(folder => folder.Value.LastCalculatedSize);

IList<Node<FolderData>> allFolders = new List<Node<FolderData>>();
rootFolder.Visit(folderNode =>
{
	allFolders.Add(folderNode);
});

const int FilesystemCapacity = 70000000;
const int DesiredFreeSize = 30000000;
const int MaxSizeOfAllFoldersAndFiles = FilesystemCapacity - DesiredFreeSize;


IList<Node<FolderData>> filteredFolders = allFolders
	.Where(folderNode => rootFolder.Value.LastCalculatedSize - folderNode.Value.LastCalculatedSize < MaxSizeOfAllFoldersAndFiles)
	.OrderBy(folderNode => folderNode.Value.LastCalculatedSize)
	.ToList();
int sizeOfFolderToRemove = filteredFolders.First().Value.LastCalculatedSize;

Console.WriteLine("Day 7A");
Console.WriteLine($"Sum of directory sizes: {sumOfSmallFolderSizes}");

Console.WriteLine("Day 7B");
Console.WriteLine($"Size of folder to remove: {sizeOfFolderToRemove}");

int CalculateFolderSize(Node<FolderData> node)
{
	node.Value.LastCalculatedSize = node.Value.Files.Sum(file => file.Size);

	foreach (var childNode in node.Children)
	{
		node.Value.LastCalculatedSize += CalculateFolderSize(childNode);
	}

	return node.Value.LastCalculatedSize;
}

bool TryParseAsGoToRoot(string row)
{
	return row == "$ cd /";
}

bool TryParseAsGoToParent(string row)
{
	return row == "$ cd ..";
}

(bool Success, string FolderName) TryParseAsGoToFolderCommand(string row)
{
	Match match = Regex.Match(row, @"^\$ cd (.+)$");

	return match.Success
		? (true, match.Groups[1].Value)
		: (false, string.Empty);
}

bool TryParseAsListCommand(string row)
{
	return row == "$ ls";
}

(bool Success, string FolderName) TryParseAsListResponseFolder(string row)
{
	Match match = Regex.Match(row, @"^dir (.+)$");

	return match.Success
		? (true, match.Groups[1].Value)
		: (false, string.Empty);
}

(bool Success, string Filename, int FileSize) TryParseAsListResponseFile(string row)
{
	Match match = Regex.Match(row, @"^(\d+) (.+)$");

	return match.Success
		? (true, match.Groups[2].Value, int.Parse(match.Groups[1].Value))
		: (false, string.Empty, 0);
}

class Node<T> where T : class, new()
{
	private IList<Node<T>> children = new List<Node<T>>();

	public Node<T>? Parent { get; }

	public IEnumerable<Node<T>> Children => children;

	public T Value { get; } = new();

	public Node(Node<T>? parent)
	{
		Parent = parent;
	}

	public Node<T> AddChild()
	{
		var newChild = new Node<T>(this);
		children.Add(newChild);

		return newChild;
	}

	public void Visit(Action<Node<T>> nodeAction)
	{
		nodeAction(this);

		foreach (var childNode in children)
		{
			childNode.Visit(nodeAction);
		}
	}
}

class FolderData
{
	public string? Name { get; set; }
	public IList<FileData> Files { get; } = new List<FileData>();

	public int LastCalculatedSize { get; set; }
}

class FileData
{
	public string? Name { get; init; }
	public int Size { get; init; }
}
