using Confix.Tool.Common.Pipelines;

namespace ConfiX.Entities.Component.Configuration.Middlewares;

public class StubExecutionContext : IExecutionContext
{
    public StubExecutionContext(string currentDirectory, string homeDirectory)
    {
        CurrentDirectory = currentDirectory;
        HomeDirectory = homeDirectory;
    }

    /// <inheritdoc />
    public string CurrentDirectory { get; set; }

    /// <inheritdoc />
    public string HomeDirectory { get; set; }
}
