using System.CommandLine.Builder;
using Confix.Tool;

namespace Confix.Utilities;

public static class GitCommandLineBuilderExtensions
{
    public static CommandLineBuilder AddGit(this CommandLineBuilder builder)
    {
        builder.AddSingleton<IGitService, GitService>();
        return builder;
    }
}
