namespace Confix.Tool.Common.Pipelines;

public interface IStatus
{
    string Message { get; set; }

    ValueTask<IAsyncDisposable> PauseAsync(CancellationToken cancellationToken);
}
