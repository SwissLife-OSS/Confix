using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Encryption;
using Confix.Tool.Schema;

namespace Confix.Tool.Pipelines.Encryption;

public sealed class FileEncryptPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseEnvironment()
            .Use<EncryptionMiddleware>()
            .AddArgument(InputFileArgument.Instance)
            .AddArgument(OutputFileArgument.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Encrypting file...");

        FileInfo inputFile = context.Parameter.Get(InputFileArgument.Instance);
        context.Parameter.TryGet(OutputFileArgument.Instance, out FileInfo? outputFile);
        var encryptionFeature = context.Features.Get<EncryptionFeature>();

        if (!inputFile.Exists)
        {
            context.Logger.FileNotExist(inputFile);
            return;
        }

        context.Logger.ReadingFile(inputFile);
        byte[] inputData = await inputFile.ReadAllBytes(context.CancellationToken);

        context.Logger.Encrypting(inputData.Length);
        byte[] encryptedData = await encryptionFeature.EncryptionProvider.EncryptAsync(inputData, context.CancellationToken);
        context.Logger.Encrypted();

        context.Logger.WritingFile(outputFile ?? inputFile);
        await using Stream outputStream = (outputFile ?? inputFile).OpenReplacementStream();
        await outputStream.WriteAsync(encryptedData);
    }
}

file static class LogExtensions
{
    public static void FileNotExist(this IConsoleLogger console, FileInfo file)
    {
        console.Error($"File {file.ToLink()} does not exist.");
    }

    public static void ReadingFile(this IConsoleLogger console, FileInfo file)
    {
        console.Debug($"Reading {file.ToLink()}");
    }

    public static void Encrypting(this IConsoleLogger console, int length)
    {
        console.Debug($"Encrypting {length} bytes");
    }

    public static void Encrypted(this IConsoleLogger console)
    {
        console.Success("Encrypted successfully");
    }

    public static void WritingFile(this IConsoleLogger console, FileInfo file)
    {
        console.Information($"Writing data to {file.ToLink()}");
    }
}