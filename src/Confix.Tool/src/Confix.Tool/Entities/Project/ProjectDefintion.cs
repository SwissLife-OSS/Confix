namespace Confix.Tool.Abstractions;

public class ProjectDefinition
{
    public ProjectDefinition(FileInfo location)
    {
        Location = location;
    }

    public FileInfo Location { get; }
}
