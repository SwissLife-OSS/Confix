using System.CommandLine;
using System.CommandLine.Invocation;

namespace Confix.Tool.Common.Pipelines;

public static class CommandExtensions
{
    public static void SetPipeline<T>(this Command command) where T : Pipeline, new()
    {
        var definition = new T();

        definition.Arguments.ForEach(command.AddArgument);
        definition.Options.ForEach(command.AddOption);

        command.SetHandler(Handler);

        async Task<int> Handler(InvocationContext context)
        {
            // create the pipeline from the definition with the binding context
            var executor = definition.BuildExecutor(context.BindingContext);

            command.Arguments.ForEach(argument =>
            {
                var value = context.ParseResult.GetValueForArgument(argument);
                executor.AddParameter(argument, value);
            });

            command.Options.ForEach(option =>
            {
                var value = context.ParseResult.GetValueForOption(option);
                executor.AddParameter(option, value);
            });

            // execute the pipeline
            return await executor.ExecuteAsync(context.GetCancellationToken());
        }
    }
}
