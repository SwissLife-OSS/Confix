using System.CommandLine;

namespace Confix.Tool;

public sealed class ArtifactFileOption : Option<FileInfo>
{
    private const string _description =
        "This parameter is used to specify the file that the 'confix' console application" +
        "generates during the build process. This file is prepared by the build pipeline and" +
        "holds essential details for the release pipeline, including the configuration" +
        "settings, variable providers, and a prebuild JSON schema for validation purposes." +
        "These features eliminate the need for an additional build and enable variable" +
        "replacement in the configuration. The argument following this parameter should be" +
        "the path to the file. This artifact file serves as a snapshot and a point of" +
        "reference for the pipeline's subsequent stages, ensuring consistency and proper" +
        "functioning.";

    public ArtifactFileOption() : base("--artifact-file", _description)
    {
    }

    public static ArtifactFileOption Instance { get; } = new();
}
