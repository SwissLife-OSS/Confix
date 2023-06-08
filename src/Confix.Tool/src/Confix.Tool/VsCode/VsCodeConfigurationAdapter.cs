using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares;

public sealed class VsCodeConfigurationAdapter : IConfigurationAdapter
{
    /// <inheritdoc />
    public async Task UpdateJsonSchemasAsync(IConfigurationAdapterContext context)
    {
        var settingsFile = context.RepositoryRoot.GetSettingsJson();

        var vsCodeSettings = VsCodeSettings.From(await ReadSettingsJson(settingsFile));

        foreach (var schema in context.Schemas)
        {
            bool Match(VsCodeJsonSchemas s)
                => schema.Project.Name == s.ProjectName &&
                    schema.RelativePathToProject == s.ProjectPath;

            var match = vsCodeSettings.Schemas.FirstOrDefault(Match);

            if (match is not null)
            {
                vsCodeSettings.Schemas.Remove(match);
            }

            var mapped = new VsCodeJsonSchemas
            {
                FileMatch = schema.FileMatch,
                ProjectName = schema.Project.Name,
                ProjectPath = schema.RelativePathToProject,
                Url = Path
                    .GetRelativePath(context.RepositoryRoot.FullName, schema.SchemaFile.FullName)
            };

            vsCodeSettings.Schemas.Add(mapped);
        }

        await WriteSettingsJson(settingsFile, vsCodeSettings);
    }

    private static async Task<JsonNode> ReadSettingsJson(FileInfo settingsFile)
    {
        JsonNode? settingsJson = null;
        if (settingsFile.Exists)
        {
            await using var stream = settingsFile.OpenRead();
            settingsJson = JsonNode.Parse(stream);
        }

        return settingsJson ?? JsonNode.Parse("{}")!;
    }

    private static async Task WriteSettingsJson(FileInfo settingsFile, VsCodeSettings settingsJson)
    {
        var currentSettings = await ReadSettingsJson(settingsFile);
        settingsJson.WriteTo(currentSettings);

        var serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = currentSettings.ToJsonString(serializerOptions);

        File.Delete(settingsFile.FullName);
        await File.WriteAllTextAsync(settingsFile.FullName, json);
    }
}
