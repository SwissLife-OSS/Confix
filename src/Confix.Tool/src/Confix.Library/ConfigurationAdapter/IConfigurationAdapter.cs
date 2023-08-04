using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode, Confix.Tool.Middlewares.IConfigurationAdapter>;

namespace Confix.Tool.Middlewares;

public interface IConfigurationAdapter
{
    Task UpdateJsonSchemasAsync(IConfigurationAdapterContext context);
}
