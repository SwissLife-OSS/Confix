
namespace Confix.Tool.Abstractions;

public class ProjectDiscovery
    : IProjectDiscovery
{
    public IEnumerable<ProjectDefinition> DiscoverProjectRecursively(string root)
    {
        var path = Path.TrimEndingDirectorySeparator(root);

        var directory = new DirectoryInfo(path);
        return DiscoverProjectRecursively(directory);
    }

    public IEnumerable<ProjectDefinition> DiscoverProjectRecursively(DirectoryInfo root)
    {
        if (!root.Exists)
        {
            throw new DirectoryNotFoundException($"The directory '{root.FullName}' was not found");
        }

        throw new NotImplementedException();
        // return Directory
        //     .EnumerateFiles(root.FullName, FileNames.ConfixProject, SearchOption.AllDirectories)
        //     .Select(path => new FileInfo(path))
        //     .Select(file => new ProjectDefinition(file));
    }
}
