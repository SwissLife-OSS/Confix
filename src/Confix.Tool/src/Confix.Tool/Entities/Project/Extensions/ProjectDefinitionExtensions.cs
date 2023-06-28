using Confix.Tool.Abstractions;
using Json.Schema;

namespace Confix.Entities.Project.Extensions;

public static class ProjectDefinitionExtensions
{
    public static JsonSchema? GetJsonSchema(this ProjectDefinition project)
    {
        if (project.JsonSchema is null)
        {
            return null;
        }

        return JsonSchema.FromText(project.JsonSchema);
    }
}
