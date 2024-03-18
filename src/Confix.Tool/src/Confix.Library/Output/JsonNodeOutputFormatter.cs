using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Reporting;

namespace Confix.Tool.Commands.Configuration;

public sealed class JsonNodeOutputFormatter : IOutputFormatter<JsonNode>
{
    /// <inheritdoc />
    public bool CanHandle(OutputFormat format, JsonNode value)
        => format == OutputFormat.Json;

    /// <inheritdoc />
    public Task<string> FormatAsync(OutputFormat format, JsonNode value)
    {
        return Task.FromResult(value.ToJsonString());
    }
}
