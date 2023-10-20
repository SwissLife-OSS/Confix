using System.Text.Json;
using System.Text.Json.Nodes;

namespace Confix.Tool.Reporting;

public sealed class DependencyProviderDefinition
{
    public DependencyProviderDefinition(string type, JsonObject value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; }

    public JsonObject Value { get; }

    public void WriteTo(Utf8JsonWriter writer) => Value.WriteTo(writer);

    public static DependencyProviderDefinition From(DependencyProviderConfiguration configuration)
    {
        var validationErrors = new List<string>();

        if (configuration.Type is null)
        {
            validationErrors.Add("Type is not defined.");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Invalid component provider configuration")
            {
                Errors = validationErrors
            };
        }

        return new DependencyProviderDefinition(
            configuration.Type!,
            configuration.Configuration);
    }
}
