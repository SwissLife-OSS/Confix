using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Entities.Components.Git;

public static class GitUrl
{
    public static string Create(string repositoryUrl, IParameterCollection parameters)
    {
        parameters.TryGet(GitUsernameOptions.Instance, out string? username);
        parameters.TryGet(GitTokenOptions.Instance, out string? token);

        if (!string.IsNullOrEmpty(token) && 
            repositoryUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(repositoryUrl);
            var builder = new UriBuilder(uri);

            if (!string.IsNullOrEmpty(username))
            {
                builder.UserName = username;
                builder.Password = token;
            }
            else
            {
                builder.UserName = token;
                builder.Password = string.Empty;
            }

            return builder.Uri.ToString();
        }

        return repositoryUrl;
    }
}