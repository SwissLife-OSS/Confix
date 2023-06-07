namespace Confix.Tool.Abstractions;

public sealed class EnvironmentDefinition
{
    public EnvironmentDefinition(
        string name,
        IReadOnlyList<string> excludeFiles,
        IReadOnlyList<string> includeFiles,
        bool enabled)
    {
        Name = name;
        ExcludeFiles = excludeFiles;
        IncludeFiles = includeFiles;
        Enabled = enabled;
    }

    public string Name { get; }

    public IReadOnlyList<string> ExcludeFiles { get; }
    public IReadOnlyList<string> IncludeFiles { get; }
    public bool Enabled { get; }

    public static EnvironmentDefinition From(EnvironmentConfiguration configuration)
    {
        var name = configuration.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Environment name is required.");
        }

        var excludeFiles = configuration.ExcludeFiles?.ToArray() ?? Array.Empty<string>();
        var includeFiles = configuration.IncludeFiles?.ToArray() ?? Array.Empty<string>();
        var enabled = configuration.Enabled ?? false;

        return new EnvironmentDefinition(name, excludeFiles, includeFiles, enabled);
    }
}
