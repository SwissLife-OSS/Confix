using System.Text.Json;
using Json.Schema;

namespace Confix.Entities.Schema;

/// <summary>
/// Handles `metadata`.
/// </summary>
public sealed class MetadataKeyword : IKeywordHandler
{
    /// <summary>
    /// The JSON name of the keyword.
    /// </summary>
    public const string Name = "metadata";

    /// <summary>
    /// The singleton instance of the handler.
    /// </summary>
    public static readonly MetadataKeyword Instance = new();

    private MetadataKeyword()
    {
    }

    /// <inheritdoc />
    string IKeywordHandler.Name => Name;

    /// <inheritdoc />
    public object? ValidateKeywordValue(JsonElement value)
    {
        if (value.ValueKind is not JsonValueKind.Array)
        {
            throw new JsonSchemaException($"'{Name}' keyword must contain an array.");
        }

        return value.Clone();
    }

    /// <inheritdoc />
    public void BuildSubschemas(KeywordData keyword, BuildContext context)
    {
    }

    /// <inheritdoc />
    public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
        => new()
        {
            Keyword = Name,
            IsValid = true,
            Annotation = (JsonElement) keyword.Value!
        };
}
