namespace Confix.Tool.Common.Pipelines;

public sealed class ExecutionContext : IExecutionContext
{
    public ExecutionContext(string currentDirectory, string homeDirectory)
    {
        CurrentDirectory = currentDirectory;
        HomeDirectory = homeDirectory;
    }

    /// <inheritdoc />
    public string CurrentDirectory { get; }

    /// <inheritdoc />
    public string HomeDirectory { get; }

    public static ExecutionContext Create()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        return new ExecutionContext(currentDirectory, homeDirectory);
    }
}
