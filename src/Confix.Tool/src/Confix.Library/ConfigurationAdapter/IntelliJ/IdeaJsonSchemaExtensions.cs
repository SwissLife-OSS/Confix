using System.Xml.Linq;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares.JsonSchemas;

namespace Confix.Tool.Middlewares;

file static class ElementNames
{
    public const string Entry = "entry";
    public const string Key = "key";
    public const string Value = "value";
    public const string SchemaInfo = "SchemaInfo";
    public const string Option = "option";
    public const string Name = "name";
    public const string GeneratedName = "generatedName";
    public const string RelativePathToSchema = "relativePathToSchema";
    public const string Patterns = "patterns";
    public const string Map = "map";
    public const string List = "list";
    public const string Item = "Item";
    public const string Path = "path";
}

public static class IdeaJsonSchemaExtensions
{
    public static void UpsertIdeaJsonSchema(
        this XDocument xmlDoc,
        JsonSchemaDefinition definition)
    {
        var name = definition.Project.Name;
        var schemaPath = Path
            .GetRelativePath(definition.Solution.FullName, definition.SchemaFile.FullName);
        var patterns = definition.FileMatch;

        var entry = xmlDoc
            .Descendants(ElementNames.Entry)
            .FirstOrDefault(e => e.GetAttributeValue(ElementNames.Key) == name);

        entry?.Remove();

        var newEntry = new XElement(ElementNames.Entry,
            new XAttribute(ElementNames.Key, name),
            new XElement(ElementNames.Value,
                new XElement(ElementNames.SchemaInfo,
                    new XElement(ElementNames.Option,
                        new XAttribute(ElementNames.Name, ElementNames.Name),
                        new XAttribute(ElementNames.Value, name)),
                    new XElement(ElementNames.Option,
                        new XAttribute(ElementNames.Name, ElementNames.GeneratedName),
                        new XAttribute(ElementNames.Value, name)),
                    new XElement(ElementNames.Option,
                        new XAttribute(ElementNames.Name, ElementNames.RelativePathToSchema),
                        new XAttribute(ElementNames.Value, schemaPath)),
                    new XElement(ElementNames.Option,
                        new XAttribute(ElementNames.Name, ElementNames.Patterns),
                        new XElement(ElementNames.List,
                            patterns.Select(p =>
                                new XElement(ElementNames.Item,
                                    new XElement(ElementNames.Option,
                                        new XAttribute(ElementNames.Name, ElementNames.Path),
                                        new XAttribute(ElementNames.Value, p)))))))));

        var map = xmlDoc
            .Descendants(ElementNames.Map)
            .FirstOrDefault();

        if (map is null)
        {
            App.Log.TheXmlFileIsInAndInvalidFormat(xmlDoc.BaseUri);
            return;
        }

        map.Add(newEntry);
    }

    private static string? GetAttributeValue(this XElement element, string attributeName)
    {
        return element.Attribute(attributeName)?.Value;
    }
}

file static class Logs
{
    public static void TheXmlFileIsInAndInvalidFormat(this IConsoleLogger logger, string xmlPath)
    {
        logger.Error($"The XML file is in {xmlPath} and invalid format.");
    }
}
