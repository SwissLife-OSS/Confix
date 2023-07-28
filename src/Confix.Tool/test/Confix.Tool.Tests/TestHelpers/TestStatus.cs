using Confix.Tool.Common.Pipelines;

namespace ConfiX.Entities.Component.Configuration.Middlewares;

public class TestStatus : IStatus
{
    /// <inheritdoc />
    public string Message { get; set; } = string.Empty;

    /// <inheritdoc />
    public ValueTask<IAsyncDisposable> PauseAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<IAsyncDisposable>(new Unpause());
    }
}

file sealed class Unpause : IAsyncDisposable
{
    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
