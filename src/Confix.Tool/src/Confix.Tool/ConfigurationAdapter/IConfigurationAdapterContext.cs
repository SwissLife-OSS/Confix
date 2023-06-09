using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares.JsonSchemas;

namespace Confix.Tool.Middlewares;

public interface IConfigurationAdapterContext
{
    CancellationToken CancellationToken { get; }

    IReadOnlyList<JsonSchemaDefinition> Schemas { get; }

    DirectoryInfo RepositoryRoot { get; }

    IConsoleLogger Logger { get; }
}
