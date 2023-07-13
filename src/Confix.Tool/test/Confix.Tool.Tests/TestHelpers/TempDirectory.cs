namespace ConfiX.Inputs;

public class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        Directory.Create();
    }

    public DirectoryInfo Directory { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Directory.Delete(true);
    }
}
