using Confix.Tool.Common.Pipelines;

namespace Confix.Entities.Component.Configuration.Middlewares;

public class StubExecutionContext : IExecutionContext
{
    public StubExecutionContext(string currentDirectory, string homeDirectory)
    {
        CurrentDirectory = new(currentDirectory);
        HomeDirectory = new(homeDirectory);
    }

    /// <inheritdoc />
    public DirectoryInfo CurrentDirectory { get; set; }

    /// <inheritdoc />
    public DirectoryInfo HomeDirectory { get; set; }
}
