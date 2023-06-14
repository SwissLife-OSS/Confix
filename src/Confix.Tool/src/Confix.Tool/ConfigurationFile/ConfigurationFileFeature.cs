namespace Confix.Tool.Middlewares;

public sealed class ConfigurationFileFeature
{
    public IList<ConfigurationFile> Files { get; set; } =
        new List<ConfigurationFile>();
}
