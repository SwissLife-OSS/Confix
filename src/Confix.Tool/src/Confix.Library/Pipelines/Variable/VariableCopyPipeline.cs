using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Variables;
using Microsoft.Extensions.DependencyInjection;

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
            .AddOption(FromVariableNameOption.Instance)
            .AddOption(ToVariableNameOption.Instance)
            .AddOption(ToEnvironmentOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        var variableContext = new VariableProviderContext(
            context.Parameter,
            context.CancellationToken);

        var variableFeature = context.Features.Get<VariablesFeature>();

        var fromResolver = variableFeature.Resolver;
        var providers = fromResolver.ListProviders().ToArray();

        var toResolver = ResolveToVariableResolver(context) ?? fromResolver;

        if (!context.Parameter.TryGet(FromVariableNameOption.Instance, out string fromVariable))
        {
            fromVariable = await context.AskAsync<string>("From variable name: ");
        }

        if (!context.Parameter.TryGet(ToVariableNameOption.Instance, out string toVariable))
        {
            toVariable = await context.AskAsync<string>("To variable name: ");
        }

        var fromVariablePath = Parse(providers, fromVariable);
        var toVariablePath = Parse(providers, toVariable);

        context.Status.Message =
            $"Copy variable {fromVariablePath.ToString().AsHighlighted()} to {toVariablePath.ToString().AsHighlighted()}...";

        var fromValue = await fromResolver.ResolveOrThrowAsync(fromVariablePath, variableContext);

        var result = await toResolver.SetVariable(toVariablePath, fromValue, variableContext);

        context.Logger.VariableSet(result);
    }

    private static IVariableResolver? ResolveToVariableResolver(IMiddlewareContext context)
    {
        var configFeature = context.Features.Get<ConfigurationFeature>();
        var variableFeature = context.Features.Get<VariablesFeature>();
        if (!context.Parameter.TryGet(ToEnvironmentOption.Instance, out string toEnvironment))
        {
            return null;
        }

        var environments = configFeature.Project?.Environments;
        if (environments?.FirstOrDefault(x => x.Name == toEnvironment) is null)
        {
            throw ThrowHelper.EnvironmentDoesNotExist(toEnvironment);
        }

        return variableFeature.CreateResolver(
            toEnvironment, 
            context.Services.GetRequiredService<VariableListCache>());
    }

    private static VariablePath Parse(
        IReadOnlyList<string> providers,
        string argumentValue)
    {
        if (!VariablePath.TryParse(argumentValue, out var parsed))
        {
            throw ThrowHelper.InvalidVariableName(argumentValue);
        }

        if (!providers.Contains(parsed.Value.ProviderName))
        {
            throw ThrowHelper.InvalidProviderName(providers, parsed.Value.ProviderName);
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
