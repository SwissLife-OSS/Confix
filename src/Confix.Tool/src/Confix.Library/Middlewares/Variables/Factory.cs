using System.Text.Json.Nodes;

namespace Confix.Tool.Middlewares;

public delegate T Factory<out T>(IServiceProvider services, JsonNode configuration);
