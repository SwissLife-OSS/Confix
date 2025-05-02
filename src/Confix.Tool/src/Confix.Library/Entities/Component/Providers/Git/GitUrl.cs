using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Entities.Components.Git;

public static class GitUrl
{
    public static string Create(string repositoryUrl, IParameterCollection parameters)
    {
        parameters.TryGet(GitUsernameOptions.Instance, out string? username);
        parameters.TryGet(GitTokenOptions.Instance, out string? token);
        
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(token))
        {
            if (repositoryUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(repositoryUrl);
                var builder = new UriBuilder(uri)
                {
                    UserName = username,
                    Password = token
                };
                return builder.Uri.ToString();
            }
        }
        
        return repositoryUrl;
    }
}