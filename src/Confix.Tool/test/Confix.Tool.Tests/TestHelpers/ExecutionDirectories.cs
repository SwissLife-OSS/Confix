using Confix.Tool.Commands.Temp;

namespace ConfiX.Inputs;

public class ExecutionDirectories : IDisposable
{
    private readonly TempDirectory _directory;

    public ExecutionDirectories()
    {
        _directory = new TempDirectory();
        Home = _directory.Directory.Append("home");
        Content = _directory.Directory.Append("content");
    }

    public DirectoryInfo Content { get; set; }

    public DirectoryInfo Home { get; set; }

    /// <inheritdoc />
    public void Dispose()
    {
        _directory.Dispose();
    }
}
