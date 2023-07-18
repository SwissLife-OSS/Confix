using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;
using HotChocolate.Types;
using Json.More;

namespace ConfiX.Variables;

public sealed class LocalVariableProvider : IVariableProvider
{
    private readonly Lazy<Dictionary<string, JsonNode?>> _parsedLocalFile;
    private readonly FileInfo _localFile;

    public LocalVariableProvider(JsonNode configuration)
        : this(LocalVariableProviderConfiguration.Parse(configuration))
    {
    }

    public LocalVariableProvider(LocalVariableProviderConfiguration configuration)
        : this(LocalVariableProviderDefinition.From(configuration))
    {
    }

    public LocalVariableProvider(LocalVariableProviderDefinition definition)
    {
        _localFile = new FileInfo(definition.Path);
        _parsedLocalFile = new(ParseConfiguration);
    }

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<string>>(_parsedLocalFile.Value.Keys.ToArray());

    public Task<JsonNode> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        if (!_localFile.Exists)
        {
            // The resolving of the variable will fail, but the file will be created. The user can
            // then edit the file. This is just a convenience for the user. Otherwise they have to
            // create the file themselves.
            App.Log.CreatingFile(_localFile);
            File.WriteAllText(_localFile.FullName, "{}");
        }

        if (_parsedLocalFile.Value.TryGetValue(path, out var value) && value is not null)
        {
            return Task.FromResult(value.Copy()!);
        }

        throw new VariableNotFoundException(path);
    }

    public Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
        => paths.ResolveMany(ResolveAsync, cancellationToken);

    public Task<string> SetAsync(string path, JsonNode value, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private Dictionary<string, JsonNode?> ParseConfiguration()
    {
        if (!_localFile.Exists)
        {
            // If the file does not exist we just return an empty dictionary. This is needed
            // for the case when the user does not have a local configuration file. 
            App.Log.ConfigFileNotFound(_localFile);
            return new Dictionary<string, JsonNode?>();
        }

        using var fileStream = _localFile.OpenRead();
        var node = JsonNode.Parse(fileStream) ?? throw new JsonException("Invalid Json Node");
        return JsonParser.ParseNode(node);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

file static class Log
{
    public static void CreatingFile(this IConsoleLogger logger, FileSystemInfo info)
    {
        logger.Warning(
            $"The local variable file was not found at the expected location. Created empty file at: {info.ToLink()}");
    }

    public static void ConfigFileNotFound(this IConsoleLogger logger, FileSystemInfo info)
    {
        logger.Debug($"Local variable file was not found at expected location: {info.ToLink()}");
    }
}
