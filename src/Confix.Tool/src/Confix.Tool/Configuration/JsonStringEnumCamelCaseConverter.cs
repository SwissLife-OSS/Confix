using System.Text.Json;
using System.Text.Json.Serialization;

namespace Confix.Extensions;

public class JsonStringEnumCamelCaseConverter : JsonStringEnumConverter
{
    public JsonStringEnumCamelCaseConverter() : base(JsonNamingPolicy.CamelCase)
    {
    }
}
