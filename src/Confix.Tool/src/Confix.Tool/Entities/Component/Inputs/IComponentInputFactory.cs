using System.Text.Json.Nodes;

namespace Confix.Tool.Entities.Component;

public interface IComponentInputFactory
{
    IComponentInput CreateInput(string type, JsonNode configuration);
}