using HotChocolate;
using HotChocolate.Language;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Schema;

public static class SchemaHelpers
{
    public static async Task<ISchema> LoadSchemaAsync(
        string schemaPath,
        CancellationToken cancellationToken = default)
    {
        if (schemaPath is null)
        {
            throw new ArgumentNullException(nameof(schemaPath));
        }

        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException(
                "The specified schema file does not exist.",
                schemaPath);
        }

        var sdl = await File.ReadAllTextAsync(schemaPath, cancellationToken);
        var schema = BuildSchema(sdl);

        return schema;
    }

    public static ISchema BuildSchema(string schema)
    {
        var schemaDoc = Utf8GraphQLParser.Parse(schema);
        var rootTypeName = schemaDoc.Definitions
            .OfType<ObjectTypeDefinitionNode>()
            .FirstOrDefault()
            ?.Name.Value ?? "Component";

        return SchemaBuilder.New()
            .AddDocument(schemaDoc)
            .Use(next => next)
            .AddType<DefaultValue>()
            .ModifyOptions(c =>
            {
                c.PreserveSyntaxNodes = true;
                c.QueryTypeName = rootTypeName;
                c.StrictValidation = false;
                c.StrictRuntimeTypeValidation = false;
            })
            .Create();
    }
}
