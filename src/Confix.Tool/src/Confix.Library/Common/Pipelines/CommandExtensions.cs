using System.CommandLine;
using System.CommandLine.Parsing;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Common.Pipelines;

public static class CommandExtensions
{
    public static void SetPipeline<T>(this Command command) where T : Pipeline, new()
    {
        var definition = new T();

        definition.Arguments.ForEach(command.Add);
        definition.Options.ForEach(command.Add);

        command.SetAction(Handler);

        async Task<int> Handler(ParseResult parseResult, CancellationToken cancellationToken)
        {
            var services = ConfixCommandLineBuilder.GetServices(parseResult);

            // verbosity has to be applied before anything is logged
            services.ApplyVerbosity(parseResult);

            return await services.ExecuteWithExceptionHandlingAsync(
                () => services.ExecuteWithOutputFormatAsync(
                    parseResult,
                    async () =>
                    {
                        // create the pipeline from the definition with the service provider
                        var executor = definition.BuildExecutor(services);

                        command.Arguments.ForEach(argument
                            => executor.AddParameter(argument, parseResult.GetValue(argument)));

                        command.Options.ForEach(option
                            => executor.AddParameter(option, parseResult.GetValue(option)));

                        // execute the pipeline
                        return await executor.ExecuteAsync(cancellationToken);
                    }));
        }
    }

    private static object? GetValue(this ParseResult parseResult, Argument argument)
        => parseResult.GetResult(argument)?.GetValueOrDefault<object?>();

    private static object? GetValue(this ParseResult parseResult, Option option)
        => parseResult.GetResult(option)?.GetValueOrDefault<object?>();
}
