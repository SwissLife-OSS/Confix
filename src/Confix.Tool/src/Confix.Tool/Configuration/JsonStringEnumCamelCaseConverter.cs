using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConfiX.Extensions;

public class JsonStringEnumCamelCaseConverter : JsonStringEnumConverter
{
    public JsonStringEnumCamelCaseConverter() : base(JsonNamingPolicy.CamelCase)
    {
    }
}
