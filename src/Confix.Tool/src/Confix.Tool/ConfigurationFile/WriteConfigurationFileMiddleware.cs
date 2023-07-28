using System.Text.Json;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares.Encryption;
using Confix.Tool.Schema;
using Confix.Utilities.Json;

namespace Confix.Tool.Middlewares;

public sealed class WriteConfigurationFileMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.SetStatus("Persisting configuration changes");

        var configurationFeature = context.Features.Get<ConfigurationFileFeature>();
        var encryptionConfigured =
            context.Features.TryGet<EncryptionFeature>(out var encryptionFeature);
        bool encryptFile = context.Parameter.Get(EncryptionOption.Instance);

        if (encryptFile && !encryptionConfigured)
        {
            throw new ExitException("Encryption must be configured to encrypt configuration files");
        }

        foreach (var file in configurationFeature.Files)
        {
            if (!file.HasContentChanged)
            {
                continue;
            }

            context.Logger.PersistingConfigurationFile(file);

            await using var stream = file.OutputFile.OpenReplacementStream();
            if (encryptFile)
            {
                context.SetStatus("Encrypting configuration file");

                await using MemoryStream memoryStream = new();
                await file.Content.SerializeToStreamAsync(memoryStream, context.CancellationToken);

                var encrypted = await encryptionFeature!.EncryptionProvider
                    .EncryptAsync(memoryStream.ToArray(), context.CancellationToken);

                await stream.WriteAsync(encrypted, context.CancellationToken);
            }
            else
            {
                await file.Content.SerializeToStreamAsync(stream, context.CancellationToken);
            }
        }

        await next(context);
    }
}

file static class Logs
{
    public static void PersistingConfigurationFile(
        this IConsoleLogger console,
        ConfigurationFile file)
    {
        console.Information($"Persisting configuration file {file.OutputFile.ToLink()}");
        console.Debug($" -> {file.OutputFile.FullName}");
    }
}
