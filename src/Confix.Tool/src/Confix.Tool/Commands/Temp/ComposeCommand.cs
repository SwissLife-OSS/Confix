using System.CommandLine;

namespace Confix.Tool.Commands.Temp;

public sealed class ComposeCommand : Command
{
    public ComposeCommand() : base("compose")
    {
        Description = "This command is temporary and will be removed in the future";
        // AddArgument(PathArgument.Instance);
        //
        // this.SetHandler(
        //     ExecuteAsync,
        //     Bind.FromServiceProvider<IAnsiConsole>(),
        //     Bind.FromServiceProvider<IProjectDiscovery>(),
        //     PathArgument.Instance,
        //     Bind.FromServiceProvider<CancellationToken>());
    }
}

// private static async Task<int> ExecuteAsync(
//         IAnsiConsole console,
//         FileInfo input,
//         CancellationToken cancellationToken)
//     {
//         var directory = input.GetDirectory();
//
//         if (!directory.Exists)
//         {
//             throw new ExitException($"The directory '{directory.FullName}' was not found");
//         }
//
//         var repository = LoadRepository(directory);
//
//         console.MarkupLine($"[yellow]Discovering projects[/] in [green]{directory.FullName}[/]");
//
//         var project = projectDiscovery.DiscoverProjectRecursively(directory.FullName);
//
//         var vsCodeConfig = new VsCodeConfig();
//         foreach (var item in project)
//         {
//             cancellationToken.ThrowIfCancellationRequested();
//
//             await ProcessProject(repository, console, item, cancellationToken);
//
//             var url = Path.GetRelativePath(
//                 repository.Location.DirectoryName!,
//                 Path.Join(repository.GetSchemasFolder(), item.GetSchemaName()));
//             var fileMatch = new List<string>()
//             {
//                 Path.GetRelativePath(repository.Location.DirectoryName!,
//                     item.Location.DirectoryName!) +
//                 "/**/*.json"
//             };
//             vsCodeConfig.JsonSchemas.Add(new JsonSchemas()
//             {
//                 FileMatch = fileMatch,
//                 Url = url
//             });
//         }
//
//         repository.GetConfixFolder().EnsureFolder();
//         var settingsPath = repository.GetSettingsJson();
//         JsonNode? settings;
//         if (File.Exists(settingsPath))
//         {
//             settings =
//                 JsonNode.Parse(await File.ReadAllTextAsync(settingsPath, cancellationToken))!;
//         }
//         else
//         {
//             settings = JsonNode.Parse("{}");
//         }
//
//         settings!["json.schemas"] = JsonSerializer
//             .SerializeToNode(vsCodeConfig.JsonSchemas, JsonSerialization.Default.ListJsonSchemas);
//
//         await File.WriteAllTextAsync(settingsPath, settings!.ToString(), cancellationToken);
//
//         return ExitCodes.Success;
//     }
//
//     private static RepositoryDefinition LoadRepository(FileSystemInfo directory)
//     {
//         var repoPath =
//             FileSystemHelpers.FindInPath(directory.FullName, FileNames.ConfixRepository);
//
//         if (repoPath is null)
//         {
//             throw new ExitException(
//                 $"Could not find repository definition in: {directory.FullName}");
//         }
//
//         return new RepositoryDefinition(new FileInfo(repoPath));
//     }
//
//     private static async Task ProcessProject(
//         RepositoryDefinition repository,
//         IAnsiConsole console,
//         ProjectDefinition project,
//         CancellationToken cancellationToken)
//     {
//         console.MarkupLine($"[green]Found project[/]: {project.Location.FullName}");
//
//         var directoryPath = project.Location.Directory?.FullName ??
//             throw new ExitException(
//                 $"Could not find directory for project: {project.Location.FullName}");
//
//         var csproj = DotnetHelpers.FindProjectFileInPath(directoryPath) ??
//             throw new ExitException($"Could not find project file in: {directoryPath}");
//
//         console.MarkupLine($"[green]Found csproj[/]: {csproj}");
//
//         await DotnetHelpers.BuildProjectAsync(csproj, cancellationToken);
//
//         var projectAssembly = DotnetHelpers.GetAssemblyFileFromCsproj(csproj) ??
//             throw new ExitException($"[red]Could not find assembly[/]: {csproj}");
//
//         var resources = DiscoverResources(console, projectAssembly, directoryPath);
//         var componentDefinitions =
//             await LoadComponentDefinitionsAsync(resources, cancellationToken);
//         var composeSchema = ComposeSchema(componentDefinitions);
//
//         var schemaPath =
//             Path.Combine(repository.GetSchemasFolder().EnsureFolder(), project.GetSchemaName());
//
//         if (File.Exists(schemaPath))
//         {
//             File.Delete(schemaPath);
//         }
//
//         await using var schema = File.OpenWrite(schemaPath);
//         await JsonSerializer
//             .SerializeAsync(schema, composeSchema, cancellationToken: cancellationToken);
//     }
//
//     private static JsonSchema ComposeSchema(IEnumerable<ComponentDefinition> componentDefinitions)
//     {
//         var defs = new Dictionary<string, JsonSchema>();
//         var properties = new Dictionary<string, JsonSchema>();
//         foreach (var componentDefinition in componentDefinitions)
//         {
//             var prefixedJsonSchema =
//                 componentDefinition.Schema.PrefixTypes($"{componentDefinition.Name}_");
//
//             if (prefixedJsonSchema.GetDefs() is { } prefixedDefs)
//             {
//                 foreach (var (name, schema) in prefixedDefs)
//                 {
//                     defs[name] = schema;
//                 }
//             }
//
//             defs[componentDefinition.Name] = new JsonSchemaBuilder()
//                 .Properties(prefixedJsonSchema.GetProperties()!)
//                 .WithDescription(prefixedJsonSchema.GetDescription())
//                 .Required(prefixedJsonSchema.GetRequired() ?? Array.Empty<string>())
//                 .AdditionalProperties(prefixedJsonSchema.GetAdditionalProperties() ?? false)
//                 .Examples(prefixedJsonSchema.GetExamples() ?? Array.Empty<JsonNode>())
//                 .Title(prefixedJsonSchema.GetTitle() ?? string.Empty)
//                 .Build();
//
//             properties[componentDefinition.Name] = new JsonSchemaBuilder()
//                 .Ref($"#/$defs/{componentDefinition.Name}")
//                 .Build();
//         }
//
//         return new JsonSchemaBuilder()
//             .Defs(defs)
//             .Properties(properties)
//             .Required(properties.Keys.ToArray())
//             .Build();
//     }
//
//     private static async Task<IReadOnlyList<ComponentDefinition>> LoadComponentDefinitionsAsync(
//         IReadOnlyList<DiscoveredResource> discoveredResources,
//         CancellationToken cancellationToken)
//     {
//         var definitions = new List<ComponentDefinition>();
//
//         var possibleComponents = discoveredResources
//             .Where(x => x.ResourceName.EndsWith(FileNames.ConfixComponent))
//             .Select(x => x.ResourceName[..^FileNames.ConfixComponent.Length])
//             .ToArray();
//
//         foreach (var component in possibleComponents)
//         {
//             var relevantResources = discoveredResources
//                 .Where(x => x.ResourceName.StartsWith(component))
//                 .ToArray();
//
//             var byAssembly = relevantResources.ToLookup(x => x.Assembly);
//
//             foreach (var resources in byAssembly)
//             {
//                 definitions.Add(await LoadComponentDefinition(resources.ToArray(), component));
//             }
//         }
//
//         return definitions;
//     }
//
//     private static async Task<ComponentDefinition> LoadComponentDefinition(
//         IReadOnlyList<DiscoveredResource> resources,
//         string component)
//     {
//         var componentConfig = resources
//             .FirstOrDefault(x => x.ResourceName.EndsWith(FileNames.ConfixComponent));
//         var jsonSchema = resources
//             .FirstOrDefault(x => x.ResourceName.EndsWith(FileNames.Schema));
//
//         if (componentConfig == null || jsonSchema == null)
//         {
//             throw new ExitException(
//                 $"[red]Could not find component definition or schema for component[/]: {component}");
//         }
//
//         await using var configStream = componentConfig.GetStream();
//
//         var configFile = JsonSerializer
//                 .Deserialize(configStream, JsonSerialization.Default.ComponentSettingsFile) ??
//             throw new ExitException(
//                 $"[red]Could not find component definition or schema for component[/]: {component}");
//
//         await using var schemaStream = jsonSchema.GetStream();
//         var schema = await JsonSchema.FromStream(schemaStream);
//
//         return new ComponentDefinition(configFile.Name, schema);
//     }
//
//     private static IReadOnlyList<DiscoveredResource> DiscoverResources(
//         IAnsiConsole console,
//         FileSystemInfo assemblyFile,
//         string directoryName)
//     {
//         var discoveredResources = new List<DiscoveredResource>();
//
//         console.MarkupLine($"[green]Found assembly[/]: {assemblyFile.Name}");
//
//         var assembliesToScan = new Queue<string>();
//         var processedAssemblies = new HashSet<string>();
//
//         assembliesToScan.Enqueue(assemblyFile.Name[..^assemblyFile.Extension.Length]);
//
//         while (assembliesToScan.TryDequeue(out var assemblyName))
//         {
//             if (!processedAssemblies.Add(assemblyName))
//             {
//                 continue;
//             }
//
//             console.MarkupLine($"[green]Scanning assembly[/]: {assemblyName}");
//
//             var assemblyFilePath = DotnetHelpers
//                 .GetAssemblyInPathByName(directoryName, assemblyName);
//
//             console.MarkupLine($"[green]Found assembly file[/]: {assemblyFilePath}");
//             if (assemblyFilePath is not { Exists: true })
//             {
//                 continue;
//             }
//
//             var assembly = Assembly.LoadFile(assemblyFilePath.FullName);
//
//             assembly
//                 .GetReferencedAssemblies()
//                 .Where(x => !string.IsNullOrWhiteSpace(x.Name))
//                 .ForEach(x => assembliesToScan.Enqueue(x.Name!));
//
//             foreach (var resourceName in assembly.GetManifestResourceNames())
//             {
//                 console.MarkupLine($"[green]Found resource[/]: {resourceName}");
//                 discoveredResources.Add(new DiscoveredResource(assembly, resourceName));
//             }
//         }
//
//         return discoveredResources;
//     }
//
//     private class DiscoveredResource
//     {
//         public DiscoveredResource(Assembly assembly, string resourceName)
//         {
//             Assembly = assembly;
//             ResourceName = resourceName;
//         }
//
//         public Assembly Assembly { get; }
//
//         public string ResourceName { get; }
//
//         public Stream GetStream() => Assembly.GetManifestResourceStream(ResourceName) ??
//             throw new ExitException($"Could not find resource: {ResourceName}");
//     }
//
//     file class PathArgument : Argument<FileInfo>
//     {
//         public static PathArgument Instance { get; } = new();
//
//         private PathArgument()
//             : base("path")
//         {
//             Arity = ArgumentArity.ExactlyOne;
//             Description = "The Path";
//         }
//     }
//
//     public static class ProjectDefinitionExtensions
//     {
//         public static string GetProjectName(this ProjectDefinition project)
//         {
//             // TODO maybe read from config?
//             return project.Location.DirectoryName!.Split(Path.DirectorySeparatorChar).Last();
//         }
//
//         public static string GetSchemaName(this ProjectDefinition project)
//         {
//             return project.GetProjectName() + ".schema.json";
//         }
//     }
