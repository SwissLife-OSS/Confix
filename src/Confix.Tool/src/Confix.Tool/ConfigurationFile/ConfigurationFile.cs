using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares;

/// <summary>
/// A json schema defines how the json schemas are configured. This is used as the source
/// for <see cref="IConfigurationAdapter"/>
/// </summary>
public sealed record ConfigurationFile
{
    private JsonNode? _content;

    public required FileInfo File { get; set; }

    [MemberNotNullWhen(true, nameof(Content))]
    public bool HasContentChanged { get; private set; }

    public JsonNode? Content
    {
        get => _content;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "Content cannot be null.");
            }

            HasContentChanged = _content != value;
            _content = value;
        }
    }

    public async ValueTask<JsonNode?> TryLoadContentAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_content is not null)
            {
                return _content;
            }

            var content = await File.ReadAllText(cancellationToken);
            _content = JsonNode.Parse(content);
            return _content;
        }
        catch (Exception ex)
        {
            App.Log.CouldNotReadFile(ex.Message);
            return null;
        }
    }
}

file static class Log
{
    public static void CouldNotReadFile(this IConsoleLogger logger, string message)
    {
        logger.Error($"Could not read file: {message}");
    }
}
