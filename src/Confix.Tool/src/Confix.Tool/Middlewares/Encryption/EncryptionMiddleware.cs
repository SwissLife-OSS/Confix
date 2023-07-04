using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.Encryption;

public sealed class EncryptionMiddleware : OptionalEncryptionMiddleware
{
    public EncryptionMiddleware(IEncryptionProviderFactory encryptionProviderFactory)
        : base(encryptionProviderFactory)
    {
    }

    public override async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        await base.InvokeAsync(context, next);

        if (!context.Features.TryGet<EncryptionFeature>(out var _))
        {
            throw new ExitException("Encryption not properly configured");
        }
    }

}
