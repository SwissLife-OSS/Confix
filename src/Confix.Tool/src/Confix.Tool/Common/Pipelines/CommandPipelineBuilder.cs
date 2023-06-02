using System.CommandLine;
using System.CommandLine.Invocation;
using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Commands.Component;

public sealed class CommandPipelineBuilder
{
    private readonly Command _command;
    private HashSet<Argument>? _arguments;
    private HashSet<Option>? _options;

    private Func<PipelineBuilder, PipelineBuilder> _pipeline = _ => _;

    public CommandPipelineBuilder(Command command)
    {
        _command = command;

        command.SetHandler(Handler, Bind.FromServiceProvider<InvocationContext>());
    }

    public CommandPipelineBuilder Use<TMiddleware>() where TMiddleware : IMiddleware
    {
        Chain(builder => builder.Use<TMiddleware>());

        return this;
    }

    public CommandPipelineBuilder AddArgument<TArgument>(TArgument argument)
        where TArgument : Argument
    {
        _arguments ??= new();

        _arguments.Add(argument);

        _command.AddArgument(argument);
        return this;
    }

    public CommandPipelineBuilder AddOption<TOption>(TOption option)
        where TOption : Option
    {
        _options ??= new();

        _options.Add(option);

        _command.AddOption(option);
        return this;
    }

    public static CommandPipelineBuilder New(Command command)
    {
        return new(command);
    }

    private void Chain(Func<PipelineBuilder, PipelineBuilder> middleware)
    {
        _pipeline = builder => middleware(_pipeline(builder));
    }

    private async Task<int> Handler(InvocationContext context)
    {
        // create the pipeline builder from the binding context
        var builder = PipelineBuilder.From(context.BindingContext);

        // add the middleware to the pipeline
        builder = _pipeline(builder);

        // build the pipeline
        var executor = builder.BuildExecutor();

        // add the parameters to the parameters collection
        executor = AddParameters(executor, context);

        // execute the pipeline
        return await executor.ExecuteAsync(context.GetCancellationToken());
    }

    private PipelineExecutor AddParameters(PipelineExecutor executor, InvocationContext context)
    {
        if (_arguments is not null)
        {
            foreach (var argument in _arguments)
            {
                executor
                    .AddParameter(argument, context.ParseResult.GetValueForArgument(argument));
            }
        }

        if (_options is not null)
        {
            foreach (var option in _options)
            {
                executor
                    .AddParameter(option, context.ParseResult.GetValueForOption(option));
            }
        }

        return executor;
    }
}
