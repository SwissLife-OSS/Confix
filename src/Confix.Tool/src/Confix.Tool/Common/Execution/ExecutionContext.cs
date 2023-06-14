namespace Confix.Tool.Common.Pipelines;

/// <inheritdoc />
public sealed class ExecutionContext : IExecutionContext
{
    /// <summary>
    /// initializes a new instance of <see cref="ExecutionContext"/>
    /// </summary>
    public ExecutionContext(string currentDirectory, string homeDirectory)
    {
        CurrentDirectory = new DirectoryInfo(currentDirectory);
        HomeDirectory = new DirectoryInfo(homeDirectory);
    }

    /// <inheritdoc />
    public DirectoryInfo CurrentDirectory { get; }

    /// <inheritdoc />
    public DirectoryInfo HomeDirectory { get; }

    /// <summary>
    /// Creates a new execution context using the current directory and home directory from the
    /// environment.
    /// </summary>
    public static ExecutionContext Create()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        return new ExecutionContext(currentDirectory, homeDirectory);
    }
}
