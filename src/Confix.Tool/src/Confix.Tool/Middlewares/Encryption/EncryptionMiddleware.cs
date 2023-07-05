using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.Encryption;

public sealed class EncryptionMiddleware : OptionalEncryptionMiddleware
{
    public EncryptionMiddleware(IEncryptionProviderFactory encryptionProviderFactory)
        : base(encryptionProviderFactory)
    {
    }

    public override Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    => base.InvokeAsync(
        context,
        (c) =>
        {
            ValidateEncryptionFeature(c);
            return next(c);
        });

    private static void ValidateEncryptionFeature(IMiddlewareContext context)
    {
        if (!context.Features.TryGet<EncryptionFeature>(out var _))
        {
            throw new ExitException("Encryption not properly configured");
        }
    }
}
