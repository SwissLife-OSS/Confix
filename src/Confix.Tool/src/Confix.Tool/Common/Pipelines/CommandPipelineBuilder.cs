using System.CommandLine;
using System.CommandLine.Invocation;
using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Builds a command pipeline that can be attached directly to a <see cref="Command"/>.
/// </summary>
/// <example>
/// Here's an example of how to use the `CommandPipelineBuilder` class:
/// <code>
/// public sealed class BuildComponentCommand : Command
/// {
///     public BuildComponentCommand() : base("build")
///         => this
///             .AddPipeline()
///             .Use&lt;LoadConfigurationMiddleware>()
///             .Use&lt;ExecuteComponentInput>()
///             .AddArgument(PathArgument.Instance)
///             .AddOption(VerboseOption.Instance)
///             .Use&lt;ExecuteComponentOutput>();
/// }
/// </code>
/// This example creates a `CommandPipelineBuilder` for a command, adds a middleware component, an
/// argument, and an option to the pipeline.
/// </example>
public sealed class CommandPipelineBuilder
{
    private readonly Command _command;
    private HashSet<Argument>? _arguments;
    private HashSet<Option>? _options;

    private Func<PipelineBuilder, PipelineBuilder> _pipeline = _ => _;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandPipelineBuilder"/> class.
    /// </summary>
    /// <param name="command">The command to which the pipeline is attached.</param>
    public CommandPipelineBuilder(Command command)
    {
        _command = command;

        command.SetHandler(Handler, Bind.FromServiceProvider<InvocationContext>());
    }

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware component.</typeparam>
    /// <returns>The current command pipeline builder instance.</returns>
    public CommandPipelineBuilder Use<TMiddleware>() where TMiddleware : IMiddleware
    {
        Chain(builder => builder.Use<TMiddleware>());

        return this;
    }

    /// <summary>
    /// Adds an argument to the command and maps it to the <see cref="IParameterCollection"/>.
    /// </summary>
    /// <typeparam name="TArgument">The type of the argument.</typeparam>
    /// <param name="argument">The argument to add.</param>
    /// <returns>The current command pipeline builder instance.</returns>
    public CommandPipelineBuilder AddArgument<TArgument>(TArgument argument)
        where TArgument : Argument
    {
        _arguments ??= new();

        _arguments.Add(argument);

        _command.AddArgument(argument);
        return this;
    }

    public CommandPipelineBuilder AddArgument<T>(string name, string description) 
        => AddArgument(new Argument<T>(name, description));

    /// <summary>
    /// Adds an option to the command and maps it to the <see cref="IParameterCollection"/>.
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    /// <param name="option">The option to add.</param>
    /// <returns>The current command pipeline builder instance.</returns>
    public CommandPipelineBuilder AddOption<TOption>(TOption option)
        where TOption : Option
    {
        _options ??= new();

        _options.Add(option);

        _command.AddOption(option);
        return this;
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

    /// <summary>
    /// Creates a new <see cref="CommandPipelineBuilder"/> for a specific command.
    /// </summary>
    /// <param name="command">The command for which to create the builder.</param>
    /// <returns>A new instance of <see cref="CommandPipelineBuilder"/>.</returns>
    public static CommandPipelineBuilder New(Command command)
    {
        return new(command);
    }
}
