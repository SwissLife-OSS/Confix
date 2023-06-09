using System.CommandLine.Builder;
using Confix.Tool;
using Confix.Tool.Entities.Components.DotNet;

namespace ConfiX.Entities.Schema.Extensions;

public static class SchemaCommandLineBuilderExtensions
{
    public static CommandLineBuilder AddSchemaServices(this CommandLineBuilder builder)
    {
        builder.AddSingleton<ISchemaStore, SchemaStore>();

        return builder;
    }
}
