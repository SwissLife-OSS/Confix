using System.CommandLine;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Temp;

public sealed class TestCommand : Command
{
    public TestCommand() : base("test")
    {
        AddArgument(PathArgument.Instance);
        AddOption(OutputFormatOption.Instance);

        this.SetHandler(
            ExecuteAsync,
            Bind.FromServiceProvider<IServiceProvider>(),
            PathArgument.Instance,
            OutputFormatOption.Instance,
            Bind.FromServiceProvider<CancellationToken>());
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider services,
        FileInfo path,
        string outputFormat,
        CancellationToken cancellationToken)
        => await PipelineBuilder
            .From(services)
            .Use<LoadConfiguration>()
            .Use<ExecuteComponentInput>()
            .Use<ExecuteComponentOutput>()
            .BuildExecutor()
            .AddParameter("path", path)
            .ExecuteAsync(cancellationToken);
}

file class PathArgument : Argument<FileInfo>
{
    public static PathArgument Instance { get; } = new();

    private PathArgument()
        : base("path")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The Path";
    }
}

file class OutputFormatOption : Option<string>
{
    public static OutputFormatOption Instance { get; } = new();

    private OutputFormatOption()
        : base("format")
    {
        Arity = ArgumentArity.ExactlyOne;
    }
}
