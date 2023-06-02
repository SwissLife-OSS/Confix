namespace Confix.Tool.Commands.Temp;

public static class FileSystemHelpers
{
    public static string? FindInPath(string directoryPath, string fileName, bool recursive = true)
        => Directory
            .EnumerateFiles(
                directoryPath,
                fileName,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

    public static string? FindInTree(string directoryPath, string fileName)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"The directory '{directoryPath}' was not found.");
        }

        var currentDirectory = directoryPath;
        while (true)
        {
            var file = Path.Combine(currentDirectory, fileName);
            if (File.Exists(file))
            {
                return file;
            }

            var parentDirectory = Directory.GetParent(currentDirectory);
            if (parentDirectory is null)
            {
                return null;
            }

            currentDirectory = parentDirectory.FullName;
        }
    }

    public static IEnumerable<string> FindAllInTree(string directoryPath, string fileName)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"The directory '{directoryPath}' was not found.");
        }

        var currentDirectory = directoryPath;
        while (true)
        {
            var file = Path.Combine(currentDirectory, fileName);
            if (File.Exists(file))
            {
                yield return file;
            }

            var parentDirectory = Directory.GetParent(currentDirectory);
            if (parentDirectory is null)
            {
                yield break;
            }

            currentDirectory = parentDirectory.FullName;
        }
    }
}
