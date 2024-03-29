namespace Confix.Tool.Common.Pipelines;

public interface IStatus : IAsyncDisposable
{
    string Message { get; set; }

    ValueTask<IAsyncDisposable> PauseAsync(CancellationToken cancellationToken);
    ValueTask StopAsync();
}
