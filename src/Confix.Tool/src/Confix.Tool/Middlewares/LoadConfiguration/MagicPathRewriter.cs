using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares;

public record MagicPathContext(
    DirectoryInfo HomeDirectory,
    DirectoryInfo? RepositoryDirectory,
    DirectoryInfo? ProjectDirectory,
    DirectoryInfo FileDirectory
);

/*
    $home,
    $repository
    $project
    ./ or .\
*/

/// <summary>
///     Rewrites the magic paths in the configuration files.
/// </summary>
public sealed class MagicPathRewriter : JsonDocumentRewriter<MagicPathContext>
{

}