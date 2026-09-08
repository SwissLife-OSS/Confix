using Confix.Tool;

namespace Confix.Utilities;

public static class GitCommandLineBuilderExtensions
{
    public static ConfixCommandLineBuilder AddGit(this ConfixCommandLineBuilder builder)
    {
        builder.AddSingleton<IGitService, GitService>();
        return builder;
    }
}
