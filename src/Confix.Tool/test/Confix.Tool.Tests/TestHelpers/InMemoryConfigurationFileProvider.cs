using Confix.Tool.Middlewares;

namespace Confix.Entities.Component.Configuration.Middlewares;

public class InMemoryConfigurationFileProvider : IConfigurationFileProvider
{
    public static string Type => "in-memory";
    
    public List<ConfigurationFile> Files { get; } = new();

    /// <inheritdoc />
    public ValueTask<IReadOnlyList<ConfigurationFile>> GetConfigurationFilesAsync(
        IConfigurationFileContext context,
        CancellationToken ct)
    {
        return ValueTask.FromResult<IReadOnlyList<ConfigurationFile>>(Files);
    }
}
