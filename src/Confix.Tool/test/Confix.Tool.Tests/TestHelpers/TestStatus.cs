using Confix.Tool.Common.Pipelines;

namespace Confix.Entities.Component.Configuration.Middlewares;

public class TestStatus : IStatus
{
    /// <inheritdoc />
    public string Message { get; set; } = string.Empty;

    /// <inheritdoc />
    public ValueTask<IAsyncDisposable> PauseAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<IAsyncDisposable>(new Unpause());
    }
    public ValueTask StopAsync() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

file sealed class Unpause : IAsyncDisposable
{
    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
