using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Validation;

/// <summary>Validates composed, resolved configuration before publication.
/// Implementations may return an advisory IDE schema when requested.</summary>
internal interface IConfigurationValidator
{
    Task<JsonNode?> ValidateAsync(IMiddlewareContext context, ValidationConfiguration settings, bool exportSchema);
}
