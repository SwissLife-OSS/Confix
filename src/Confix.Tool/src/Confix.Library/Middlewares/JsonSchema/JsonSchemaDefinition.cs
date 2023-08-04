using Confix.Tool.Abstractions;

namespace Confix.Tool.Middlewares.JsonSchemas;

/// <summary>
/// A json schema defines how the json schemas are configured. This is used as the source
/// for <see cref="IConfigurationAdapter"/>
/// </summary>
public sealed class JsonSchemaDefinition
{
    public required ProjectDefinition Project { get; set; }

    public required FileInfo SchemaFile { get; set; }

    public required DirectoryInfo Solution { get; set; }

    public required string RelativePathToProject { get; set; }

    public required IList<string> FileMatch { get; set; }
    
}
