namespace Confix.Tool.Abstractions;

public sealed class EnvironmentDefinition
{
    public EnvironmentDefinition(string name, IReadOnlyList<string> excludeFiles)
    {
        Name = name;
        ExcludeFiles = excludeFiles;
    }

    public string Name { get; }

    public IReadOnlyList<string> ExcludeFiles { get; }

    public static EnvironmentDefinition From(EnvironmentConfiguration configuration)
    {
        var name = configuration.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Environment name is required.");
        }

        var excludeFiles = configuration.ExcludeFiles?.ToArray() ?? Array.Empty<string>();

        return new EnvironmentDefinition(name, excludeFiles);
    }
}
