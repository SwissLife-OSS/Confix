using System.Text;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
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

        var missingScalarDefinitions = GetMissingScalarDefinitions(schemaDoc).ToList();
        if (missingScalarDefinitions.Count > 0)
        {
            foreach (var missingScalarDefinition in missingScalarDefinitions)
            {
                schema += Environment.NewLine + $"scalar {missingScalarDefinition}";
            }

            schemaDoc = Utf8GraphQLParser.Parse(schema);
        }

        var rootTypeName = schemaDoc.Definitions
            .OfType<ObjectTypeDefinitionNode>()
            .FirstOrDefault()
            ?.Name.Value ?? "Component";

        var builder = SchemaBuilder.New()
            .AddDocument(schemaDoc)
            .Use(next => next)
            .AddType<DefaultValue>()
            .ModifyOptions(c =>
            {
                c.PreserveSyntaxNodes = true;
                c.QueryTypeName = rootTypeName;
                c.StrictValidation = false;
                c.StrictRuntimeTypeValidation = false;
            });

        foreach (var scalar in schemaDoc.Definitions.OfType<ScalarTypeDefinitionNode>())
        {
            if (!Scalars.IsBuiltIn(scalar.Name.Value))
            {
                builder.AddType(new AnyType(scalar.Name.Value, scalar.Description?.Value));
            }
            else if (scalar.Name.Value == ScalarNames.Any)
            {
                builder.AddType(new AnyType());
            }
        }

        return builder.Create();
    }

    private static IEnumerable<string> GetMissingScalarDefinitions(DocumentNode documentNode)
    {
        var usedTypes = GetAllUsedTypes(documentNode);
        var definedTypes = GetAllDefinedTypes(documentNode);

        var missingTypes = usedTypes.Except(definedTypes).ToList();

        if (missingTypes.Count == 0)
        {
            yield break;
        }

        foreach (var missingType in missingTypes)
        {
            if (Scalars.IsBuiltIn(missingType))
            {
                continue;
            }

            yield return missingType;
        }
    }

    private static HashSet<string> GetAllUsedTypes(DocumentNode documentNode)
    {
        return documentNode.Definitions.SelectMany(x => x switch
            {
                ObjectTypeDefinitionNode n => n.Fields.Select(x => x.Type.NamedType().Name.Value),
                InterfaceTypeDefinitionNode n =>
                    n.Fields.Select(x => x.Type.NamedType().Name.Value),
                InputObjectTypeDefinitionNode n => n.Fields.Select(x
                    => x.Type.NamedType().Name.Value),
                _ => Enumerable.Empty<string>()
            })
            .ToHashSet();
    }

    private static HashSet<string> GetAllDefinedTypes(DocumentNode documentNode)
    {
        return documentNode.Definitions
            .OfType<NamedSyntaxNode>()
            .Select(x => x.Name.Value)
            .ToHashSet();
    }
}
