using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Encryption;
using Confix.Tool.Schema;

namespace Confix.Tool.Pipelines.Encryption;

public sealed class FileDecryptPipeline : Pipeline
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
        context.SetStatus("Decrypting file...");

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

        context.Logger.Decrypting(inputData.Length);
        byte[] encryptedData = await encryptionFeature.EncryptionProvider.DecryptAsync(inputData, context.CancellationToken);
        context.Logger.Decrypted();

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

    public static void Decrypting(this IConsoleLogger console, int length)
    {
        console.Debug($"Decrypting {length} bytes");
    }

    public static void Decrypted(this IConsoleLogger console)
    {
        console.Success("Decrypted successfully");
    }

    public static void WritingFile(this IConsoleLogger console, FileInfo file)
    {
        console.Information($"Writing data to {file.ToLink()}");
    }
}