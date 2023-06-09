using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares.JsonSchemas;

namespace Confix.Tool.Middlewares;

public sealed class ConfigurationAdapterContext
    : IConfigurationAdapterContext
{
    public required CancellationToken CancellationToken { get; init; }

    public required IReadOnlyList<JsonSchemaDefinition> Schemas { get; init; }

    public required DirectoryInfo RepositoryRoot { get; init; }

    public required IConsoleLogger Logger { get; init; }
}
