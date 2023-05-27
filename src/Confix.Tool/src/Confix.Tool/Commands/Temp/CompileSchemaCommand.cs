using System.CommandLine;
using System.Text.Json;
using Confix.Tool.Schema;
using Json.Schema;
using Spectre.Console;

namespace Confix.Tool.Commands.Temp;

public sealed class CompileSchemaCommand : Command
{
    public CompileSchemaCommand() : base("compile-schema")
    {
        Description = "This command is temporary and will be removed in the future";
        AddOption(InputOption.Instance);
        AddOption(OutputOption.Instance);

        this.SetHandler(
            ExecuteAsync,
            Bind.FromServiceProvider<IAnsiConsole>(),
            InputOption.Instance,
            OutputOption.Instance,
            Bind.FromServiceProvider<CancellationToken>());
    }

    private static async Task<int> ExecuteAsync(
        IAnsiConsole console,
        FileInfo input,
        FileInfo? output,
        CancellationToken cancellationToken)
    {
        if (!input.Exists)
        {
            throw new ExitException($"Input file '{input.FullName}' does not exist");
        }

        output ??= new FileInfo(Path.Combine(input.DirectoryName!, "schema.json"));

        var schema = await SchemaHelpers.LoadSchemaAsync(input.FullName, cancellationToken);

        var jsonSchema = schema.ToJsonSchema().Build();

        if (output.Exists)
        {
            output.Delete();
        }

        await using var fileStream = File.OpenWrite(output.FullName);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        await JsonSerializer.SerializeAsync(fileStream, jsonSchema, options, cancellationToken);

        return ExitCodes.Success;
    }
}

file class InputOption : Option<FileInfo>
{
    public InputOption() : base(new[] { "--input", "-i" }, "The input file")
    {
        IsRequired = true;
    }

    public static InputOption Instance { get; } = new();
}

file class OutputOption : Option<FileInfo>
{
    public OutputOption() : base(new[] { "--output", "-o" }, "The output file")
    {
    }

    public static OutputOption Instance { get; } = new();
}
