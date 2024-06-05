using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Schema;
using Json.Schema;
using Spectre.Console;
using Exception = System.Exception;

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

        var projectAssembly = DotnetHelpers.GetAssemblyNameFromCsproj(csproj);
        var components = await DiscoverResources(context.Logger, projectAssembly, projectDirectory);
        foreach (var component in components)
        {
            context.Components.Add(component);
        }
    }

    private static async Task<IReadOnlyList<Component>> DiscoverResources(
        IConsoleLogger logger,
        string rootAssemblyName,
        DirectoryInfo directory)
    {
        var discoveredResources = new List<DiscoveredResource>();

        logger.FoundAssembly(rootAssemblyName);

        var assembliesToScan = new Queue<string>();
        var processedAssemblies = new HashSet<string>();

        assembliesToScan.Enqueue(rootAssemblyName);

        var assemblyResolver = DotnetHelpers.CreateAssemblyResolver(directory);
        using var metadataLoadContext = new MetadataLoadContext(assemblyResolver);

        while (assembliesToScan.TryDequeue(out var assemblyName))
        {
            if (!processedAssemblies.Add(assemblyName))
            {
                continue;
            }

            logger.ScanningAssembly(assemblyName);

            try
            {
                var assembly = metadataLoadContext.TryLoadAssembly(assemblyName);
                if (assembly is null)
                {
                    logger.AssemblyFileNotFound(assemblyName);
                    continue;
                }

                var isComponentRoot = assembly.IsComponentRoot();
                if (isComponentRoot)
                {
                    logger.DetectedComponentRoot(assemblyName);
                }
                else
                {
                    var referencedAssemblies = assembly
                        .GetReferencedAssemblies()
                        .Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                                    !x.Name.StartsWith("System", StringComparison.InvariantCulture) &&
                                    !x.Name.StartsWith("Microsoft", StringComparison.InvariantCulture) &&
                                    !x.Name.StartsWith("mscorlib", StringComparison.InvariantCulture))
                        .ToArray();

                    referencedAssemblies.ForEach(x => assembliesToScan.Enqueue(x.Name!));
                }

                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    logger.FoundManifestResourceInAssembly(resourceName, assemblyName);
                    discoveredResources.Add(new DiscoveredResource(assembly, resourceName));
                }
            }
            catch (Exception ex)
            {
                logger.CouldNotLoadAssembly(assemblyName, ex);
            }
        }

        return await LoadComponents(discoveredResources);
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
                var fvi = FileVersionInfo.GetVersionInfo(resources.Key.Location);
                var version = fvi.FileVersion;
                components.Add(await LoadComponent(version, resources.ToArray(), component));
            }
        }

        return components;
    }

    private static async Task<Component> LoadComponent(
        string? version,
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
            version,
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

    private record DiscoveredResource(Assembly Assembly, string ResourceName)
    {
        public Stream GetStream() => Assembly.GetManifestResourceStream(ResourceName) ??
            throw new ExitException(
                $"Could not find resource: {ResourceName}");
    }
}

file static class Extensions
{
    public static Assembly? TryLoadAssembly(this MetadataLoadContext context, string assemblyName)
    {
        try
        {
            return context
                    .GetAssemblies()
                    .FirstOrDefault(x => x.FullName == assemblyName) ??
                context.LoadFromAssemblyName(assemblyName);
        }
        catch
        {
            return null;
        }
    }

    public static bool IsComponentRoot(this Assembly assembly)
    {
        return assembly
            .GetCustomAttributesData()
            .Any(x =>
            {
                try
                {
                    // even though both are assembly metadata attributes, they are not of the equal
                    // type, so we need to compare the full name
                    return x.AttributeType.FullName ==
                        typeof(AssemblyMetadataAttribute).FullName &&
                        x.ConstructorArguments is
                        [
                            { Value: "IsConfixComponentRoot" }, { Value: "true" }
                        ];
                }
                catch
                {
                    return false;
                }
            });
    }

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

    public static void AssemblyFileNotFound(this IConsoleLogger logger, string assembly)
    {
        logger.Debug($"Assembly file not found for assembly: {assembly}");
    }

    public static void FoundAssembly(this IConsoleLogger logger, string assemblyName)
    {
        logger.Debug($"Found assembly: {assemblyName}");
    }

    public static void ScanningAssembly(this IConsoleLogger logger, string assembly)
    {
        logger.Debug($"Scanning assembly: {assembly}");
    }

    public static void CouldNotLoadAssembly(
        this IConsoleLogger logger,
        string assembly,
        Exception ex)
    {
        logger.Debug($"Could not load assembly: {assembly}. {ex.Message}");
    }

    public static void FoundDotnetProject(this IConsoleLogger logger, FileSystemInfo csproj)
    {
        logger.Debug($"Found .NET project:{csproj.ToLink()} [dim]{csproj.FullName}[/]");
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

    public static void DetectedComponentRoot(
        this IConsoleLogger logger,
        string assembly)
    {
        logger.Inform(
            $"Detected component root in assembly '{assembly}'. Skipping scanning referenced assemblies.");
    }
}
