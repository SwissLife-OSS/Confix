using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using ConfiX.Variables;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableCopyPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseEnvironment()
            .Use<VariableMiddleware>()
            .AddArgument(FromVariableNameArgument.Instance)
            .AddArgument(ToVariableNameArgument.Instance)
            .AddOption(ToEnvironmentOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        var cancellationToken = context.CancellationToken;

        var variableFeature = context.Features.Get<VariableResolverFeature>();

        var fromResolver = variableFeature.Resolver;
        var providers = fromResolver.ListProviders().ToArray();

        var toResolver = ResolveToVariableResolver(context) ?? fromResolver;

        var fromVariable = context.Parameter.Get(FromVariableNameArgument.Instance);
        var toVariable = context.Parameter.Get(ToVariableNameArgument.Instance);

        var fromVariablePath = Parse(providers, fromVariable);
        var toVariablePath = Parse(providers, toVariable);

        var fromValue = await fromResolver.ResolveOrThrowAsync(fromVariablePath, cancellationToken);

        var result = await toResolver.SetVariable(toVariablePath, fromValue, cancellationToken);

        context.Logger.VariableSet(result);
    }

    private static IVariableResolver? ResolveToVariableResolver(IMiddlewareContext context)
    {
        var configFeature = context.Features.Get<ConfigurationFeature>();
        var variableFeature = context.Features.Get<VariableResolverFeature>();
        if (!context.Parameter.TryGet(ToEnvironmentOption.Instance, out string toEnvironment))
        {
            return null;
        }

        var environments = configFeature.Project?.Environments;
        if (environments?.FirstOrDefault(x => x.Name == toEnvironment) is null)
        {
            throw new ExitException($"Environment '{toEnvironment}' does not exists.")
            {
                Help = $"Use [blue]confix environment set {toEnvironment}[/] to change it."
            };
        }

        return variableFeature.CreateResolver(toEnvironment);
    }

    private static VariablePath Parse(
        IReadOnlyList<string> providers,
        string argumentValue)
    {
        if (!VariablePath.TryParse(argumentValue, out var parsed))
        {
            throw new ExitException($"Invalid variable name: {argumentValue}")
            {
                Help = "Variable name must be like: [blue]$provider:some.path[/]"
            };
        }

        if (!providers.Contains(parsed.Value.ProviderName))
        {
            throw new ExitException($"Invalid provider name: {parsed.Value.ProviderName}")
            {
                Help = $"Available providers: {string.Join(", ", providers)}"
            };
        }

        return parsed.Value;
    }
}

file static class Log
{
    public static void VariableSet(
        this IConsoleLogger console,
        VariablePath variablePath)
    {
        console.Success($"Variable [green]{variablePath}[/] set successfully.");
    }
}
