using System.Reflection;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Schema;
using Json.Schema;
using Spectre.Console;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class DotnetPackageComponentProvider : IComponentProvider
{
    public static string Type => "dotnet-package";

    /// <inheritdoc />
    public async Task ExecuteAsync(IComponentProviderContext context)
    {
        context.Logger.StartLoadingComponents(context.Project.Name);

        var projectDirectory = context.EnsureProject();
        context.EnsureSolution();

        var csproj = DotnetHelpers.FindProjectFileInPath(projectDirectory);

        if (csproj is null)
        {
            context.Logger.ProjectNotFoundInDirectory(projectDirectory);
            context.Logger.DotnetProjectWasNotDetected();
            return;
        }

        context.Logger.FoundDotnetProject(csproj);

        var buildResult = await DotnetHelpers.BuildProjectAsync(csproj, context.CancellationToken);
        if (!buildResult.Succeeded)
        {
            var output = buildResult.Output.EscapeMarkup();
            throw new ExitException($"Failed to build project:\n{output}");
        }

        var projectAssembly = DotnetHelpers.GetAssemblyFileFromCsproj(csproj);

        if (projectAssembly is not { Exists: true })
        {
            context.Logger.ProjectNotFoundInDirectory(projectDirectory);
            context.Logger.DotnetProjectWasNotDetected();
            return;
        }

        var resources =
            DiscoverResources(context.Logger, projectAssembly, projectDirectory);
        var components = await LoadComponents(resources);
        foreach (var component in components)
        {
            context.Components.Add(component);
        }
    }

    private static async Task<IReadOnlyList<Component>> LoadComponents(
        IReadOnlyList<DiscoveredResource> discoveredResources)
    {
        var components = new List<Component>();

        var possibleComponents = discoveredResources
            .Where(x => x.ResourceName.EndsWith(FileNames.ConfixComponent))
            .Select(x => x.ResourceName[..^FileNames.ConfixComponent.Length])
            .ToArray();

        foreach (var component in possibleComponents)
        {
            var relevantResources = discoveredResources
                .Where(x => x.ResourceName.StartsWith(component))
                .ToArray();

            var byAssembly = relevantResources.ToLookup(x => x.Assembly);

            foreach (var resources in byAssembly)
            {
                components.Add(await LoadComponent(resources.ToArray(), component));
            }
        }

        return components;
    }

    private static async Task<Component> LoadComponent(
        IReadOnlyList<DiscoveredResource> resources,
        string component)
    {
        var componentConfig = resources
            .FirstOrDefault(x => x.ResourceName.EndsWith(FileNames.ConfixComponent));
        var jsonSchema = resources
            .FirstOrDefault(x => x.ResourceName.EndsWith(FileNames.Schema));

        if (componentConfig == null || jsonSchema == null)
        {
            throw new ExitException(
                $"[red]Could not find component definition or schema for component[/]: {component}");
        }

        App.Log.ParsingComponent(componentConfig.Assembly, componentConfig.ResourceName);
        var componentConfiguration = await LoadComponentConfigurationFromAssembly(componentConfig);

        await using var schemaStream = jsonSchema.GetStream();
        var schema = await JsonSchema.FromStream(schemaStream);

        if (componentConfiguration.Name is null)
        {
            throw new ExitException($"Component definition is missing name: {component}");
        }

        return new Component(
            Type,
            componentConfiguration.Name,
            null,
            true,
            new[] { componentConfiguration.Name },
            schema);
    }

    private static async ValueTask<ComponentConfiguration> LoadComponentConfigurationFromAssembly(
        DiscoveredResource componentConfig)
    {
        await using var configStream = componentConfig.GetStream();

        try
        {
            return ComponentConfiguration.Parse(JsonNode.Parse(configStream));
        }
        catch
        {
            var resource = componentConfig.ResourceName;
            var assembly = componentConfig.Assembly.FullName;

            throw new ExitException(
                $"Could not parse component definition: {resource} in assembly: {assembly}");
        }
    }

    private static IReadOnlyList<DiscoveredResource> DiscoverResources(
        IConsoleLogger logger,
        FileSystemInfo assemblyFile,
        DirectoryInfo directory)
    {
        var discoveredResources = new List<DiscoveredResource>();

        logger.FoundAssembly(assemblyFile);

        var assembliesToScan = new Queue<string>();
        var processedAssemblies = new HashSet<string>();

        assembliesToScan.Enqueue(assemblyFile.Name[..^assemblyFile.Extension.Length]);

        while (assembliesToScan.TryDequeue(out var assemblyName))
        {
            if (!processedAssemblies.Add(assemblyName))
            {
                continue;
            }

            logger.ScanningAssembly(assemblyName);

            var assemblyFilePath = DotnetHelpers
                .GetAssemblyInPathByName(directory, assemblyName);

            if (assemblyFilePath is not { Exists: true })
            {
                logger.AssemblyFileNotFound(assemblyName);
                continue;
            }

            try
            {
                logger.FoundAssemblyFile(assemblyFilePath);
                var assembly = Assembly.LoadFile(assemblyFilePath.FullName);

                assembly
                    .GetReferencedAssemblies()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                                !x.Name.StartsWith("System", StringComparison.InvariantCulture) &&
                                !x.Name.StartsWith("Microsoft", StringComparison.InvariantCulture))
                    .ForEach(x => assembliesToScan.Enqueue(x.Name!));

                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    logger.FoundManifestResourceInAssembly(resourceName, assemblyName);
                    discoveredResources.Add(new DiscoveredResource(assembly, resourceName));
                }
            }
            catch (BadImageFormatException ex)
            {
                logger.CouldNotLoadAssembly(assemblyFile, ex);
            }
        }

        return discoveredResources;
    }

    private record DiscoveredResource(Assembly Assembly, string ResourceName)
    {
        public Stream GetStream() => Assembly.GetManifestResourceStream(ResourceName) ??
                                     throw new ExitException(
                                         $"Could not find resource: {ResourceName}");
    }
}

file static class Extensions
{
    public static void EnsureSolution(this IComponentProviderContext context)
    {
        if (context.Solution.Directory is not { Exists: true })
        {
            throw new ExitException("A solution directory is required to load components");
        }
    }

    public static DirectoryInfo EnsureProject(this IComponentProviderContext context)
    {
        if (context.Project.Directory is not { Exists: true } directoryInfo)
        {
            throw new ExitException("A project directory is required to load components");
        }

        return directoryInfo;
    }
}

file static class Log
{
    public static void StartLoadingComponents(this IConsoleLogger logger, string name)
    {
        logger.Debug($"Start loading components from project '{name}'");
    }

    public static void FoundAssembly(this IConsoleLogger logger, FileSystemInfo assembly)
    {
        logger.Debug($"Found assembly: {assembly.Name}");
    }

    public static void ScanningAssembly(this IConsoleLogger logger, string assembly)
    {
        logger.Debug($"Scanning assembly: {assembly}");
    }

    public static void FoundAssemblyFile(this IConsoleLogger logger, FileSystemInfo assembly)
    {
        logger.Debug($"Found assembly file: {assembly.FullName}");
    }

    public static void CouldNotLoadAssembly(
        this IConsoleLogger logger,
        FileSystemInfo assembly, Exception ex)
    {
        logger.Debug($"Could not load assembly: {assembly.FullName}. {ex.Message}");
    }

    public static void AssemblyFileNotFound(this IConsoleLogger logger, string assembly)
    {
        logger.Debug($"Assembly file not found for assembly: {assembly}");
    }

    public static void FoundDotnetProject(this IConsoleLogger logger, FileSystemInfo csproj)
    {
        logger.Information($"Found .NET project:{csproj.ToLink()} [dim]{csproj.FullName}[/]");
    }

    public static void FoundManifestResourceInAssembly(
        this IConsoleLogger logger,
        string assembly,
        string manifest)
    {
        logger.Debug($"Found manifest resource in assembly '{assembly}': {manifest}");
    }

    public static void ProjectNotFoundInDirectory(
        this IConsoleLogger logger,
        DirectoryInfo directory)
    {
        logger.Debug($"Could not find project in directory: {directory}");
    }

    public static void DotnetProjectWasNotDetected(this IConsoleLogger logger)
    {
        logger.Information(".NET Project was not detected");
    }

    public static void ParsingComponent(
        this IConsoleLogger logger,
        Assembly assembly,
        string resourceName)
    {
        logger.Debug(
            $"Parsing component from resource '{resourceName}' in assembly '{assembly.FullName}'");
    }
}
