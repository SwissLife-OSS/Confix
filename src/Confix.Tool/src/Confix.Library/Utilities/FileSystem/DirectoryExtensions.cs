namespace Confix.Tool.Commands.Temp;

public static class DirectoryExtensions
{
    public static FileInfo? FindInPath(
        this DirectoryInfo? directory,
        string fileName,
        bool recursive = true)
        => directory is not null
            ? Directory
                .EnumerateFiles(
                    directory.FullName,
                    fileName,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(x => new FileInfo(x))
                .FirstOrDefault()
            : null;

    public static IEnumerable<FileInfo> FindAllInPath(
        this DirectoryInfo? directory,
        string pattern,
        bool recursive = true)
        => directory is not null
            ? Directory
                .EnumerateFiles(
                    directory.FullName,
                    pattern,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(x => new FileInfo(x))
            : Array.Empty<FileInfo>();

    public static string? FindInTree(this DirectoryInfo? directory, string fileName)
    {
        if (directory is null)
        {
            return null;
        }

        if (!directory.Exists)
        {
            throw new DirectoryNotFoundException($"The directory '{directory}' was not found.");
        }

        var currentDirectory = directory.FullName;
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

    public static IEnumerable<FileInfo> FindAllInTree(this DirectoryInfo directory, string fileName)
    {
        if (!directory.Exists)
        {
            throw new DirectoryNotFoundException(
                $"The directory '{directory.FullName}' was not found.");
        }

        var currentDirectory = directory.FullName;
        while (true)
        {
            var file = Path.Combine(currentDirectory, fileName);
            if (File.Exists(file))
            {
                yield return new FileInfo(file);
            }

            var parentDirectory = Directory.GetParent(currentDirectory);
            if (parentDirectory is null)
            {
                yield break;
            }

            currentDirectory = parentDirectory.FullName;
        }
    }

    public static DirectoryInfo Append(this DirectoryInfo directory, string path)
        => new(Path.Join(directory.FullName, path));

    public static FileInfo AppendFile(this DirectoryInfo directory, string path)
        => new(Path.Join(directory.FullName, path));

    public static async Task WriteAllTextAsync(
        this FileInfo file,
        string content,
        CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenWrite();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
    }
}
