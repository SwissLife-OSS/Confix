using Confix.Tool.Middlewares.Project;
using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode, Confix.Tool.Middlewares.IConfigurationFileProvider>;

namespace Confix.Tool.Middlewares;

public interface IConfigurationFileProvider
{
    public static virtual string Type => string.Empty;
    
    IReadOnlyList<ConfigurationFile> GetConfigurationFiles(IConfigurationFileContext context);
}
