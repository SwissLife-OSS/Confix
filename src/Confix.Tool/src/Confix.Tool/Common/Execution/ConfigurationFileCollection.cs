using System.Collections;
using Confix.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Abstractions.Configuration;

namespace Confix.Tool.Middlewares;

public sealed class ConfigurationFileCollection
    : IConfigurationFileCollection
{
    private readonly IReadOnlyList<JsonFile> _collection;

    public ConfigurationFileCollection(
        RuntimeConfiguration? configuration,
        SolutionConfiguration? solutionConfiguration,
        ProjectConfiguration? projectConfiguration,
        ComponentConfiguration? componentConfiguration,
        IReadOnlyList<JsonFile> collection)
    {
        RuntimeConfiguration = configuration;
        Solution = solutionConfiguration;
        Project = projectConfiguration;
        Component = componentConfiguration;
        _collection = collection;
    }

    public RuntimeConfiguration? RuntimeConfiguration { get; }

    public SolutionConfiguration? Solution { get; }

    public ProjectConfiguration? Project { get; }

    public ComponentConfiguration? Component { get; }

    /// <inheritdoc />
    public IEnumerator<JsonFile> GetEnumerator()
        => _collection.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc />
    public int Count => _collection.Count;

    /// <inheritdoc />
    public JsonFile this[int index] => _collection[index];
}
