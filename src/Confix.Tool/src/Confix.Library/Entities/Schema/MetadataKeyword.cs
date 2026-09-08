using System.Runtime.CompilerServices;
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

    private static int _registered;

    private MetadataKeyword()
    {
    }

    /// <summary>
    /// Registers the handler on <see cref="Dialect.Default"/>. Calling this multiple times
    /// has no additional effect.
    /// </summary>
    public static void Register()
    {
        if (Interlocked.Exchange(ref _registered, 1) is 0)
        {
            // Confix schemas carry additional annotations (for example `hasVariable`) that are
            // not backed by a handler, so unknown keywords have to be tolerated.
            Dialect.Default = Dialect.Default.With([Instance], allowUnknownKeywords: true);
            BuildOptions.Default.Dialect = Dialect.Default;
        }
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

/// <summary>
/// Registers the <see cref="MetadataKeyword"/> handler on the default dialect so that the
/// keyword is recognized when schemas are built and evaluated.
/// </summary>
internal static class MetadataKeywordRegistration
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        MetadataKeyword.Register();
    }
}
