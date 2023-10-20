using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;
using Json.Schema;

namespace Confix.Tool.Reporting;

public sealed class RegexDependencyProvider : IDependencyProvider
{
    private readonly Regex _regex;
    private readonly string _kind;

    public static string Type => "regex";

    public RegexDependencyProvider(JsonNode node)
        : this(RegexDependencyProviderConfiguration.Parse(node))
    {
    }

    public RegexDependencyProvider(RegexDependencyProviderConfiguration configuration)
        : this(RegexDependencyProviderDefinition.From(configuration))
    {
    }

    public RegexDependencyProvider(RegexDependencyProviderDefinition definition)
    {
        _regex = new Regex(definition.Regex, RegexOptions.Compiled);
        _kind = definition.Kind;
    }

    public void Analyze(DependencyAnalyzerContext context, JsonNode node)
    {
        if (node.GetSchemaValueType() is not SchemaValueType.String)
        {
            return;
        }

        var value = node.AsValue().GetValue<string>();

        var match = _regex.Match(value);

        if (match.Success)
        {
            var dependency =
                new RegexDependency(Type, _kind, node.GetPointerFromRoot(), match.Groups);
            context.AddDependency(dependency);
        }
    }
}
