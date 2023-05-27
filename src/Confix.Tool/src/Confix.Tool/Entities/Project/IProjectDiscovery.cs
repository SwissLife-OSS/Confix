namespace Confix.Tool.Abstractions;

public interface IProjectDiscovery
{
    IEnumerable<ProjectDefinition> DiscoverProjectRecursively(string root);
}
