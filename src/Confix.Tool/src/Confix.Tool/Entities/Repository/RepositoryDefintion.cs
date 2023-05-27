namespace Confix.Tool.Abstractions;

public class RepositoryDefinition
{
    public RepositoryDefinition(FileInfo location)
    {
        Location = location;
    }

    public FileInfo Location { get; }
}
