using System.Net.NetworkInformation;
using Confix.Tool.Commands.Logging;
using Spectre.Console;

namespace Confix.Tool.Common.Pipelines;

public sealed class SpectreStatusContext : IStatus, IAsyncDisposable
{
    private readonly IAnsiConsole _console;

    private string _statusText = "...";
    private DisposableStatus? _status;

    public SpectreStatusContext(IAnsiConsole console)
    {
        _console = console;
    }

    public string Message
    {
        get => _statusText;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
            }

            _statusText = value;
            if (_status != null)
            {
                _status.Context.Status = value;
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StopAsync();
        _status = new DisposableStatus(_console);
        await _status.StartAsync(_statusText, cancellationToken);
    }

    public async ValueTask StopAsync()
    {
        if (_status is not null)
        {
            await _status.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async ValueTask<IAsyncDisposable> PauseAsync(CancellationToken cancellationToken)
    {
        await StopAsync();
        return new Unpause(this, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_status is not null)
        {
            await _status.DisposeAsync();
        }

        _status = null;
    }

    private class Unpause : IAsyncDisposable
    {
        private readonly SpectreStatusContext _context;
        private readonly CancellationToken _cancellationToken;

        public Unpause(SpectreStatusContext context, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _context = context;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _context.StartAsync(_cancellationToken);
        }
    }

    private sealed class DisposableStatus : IAsyncDisposable
    {
        private readonly TaskCompletionSource _completion;
        private readonly TaskCompletionSource _initialization;
        private readonly IAnsiConsole _console;
        private StatusContext? _context;
        private bool _disposed;
        private Task? _task;

        public DisposableStatus(IAnsiConsole console)
        {
            _initialization = new TaskCompletionSource();
            _completion = new TaskCompletionSource();
            _console = console;
        }

        public async Task StartAsync(string status, CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DisposableStatus));
            }

            _task = _console
                .Status()
                .StartAsync(status,
                    async ctx =>
                    {
                        _initialization.SetResult();
                        _context = ctx;
                        await _completion.Task;
                    });
            await _initialization.Task.WaitAsync(cancellationToken);
            Context.Status = status;
        }

        public StatusContext Context
        {
            get
            {
                if (_context is null)
                {
                    throw new InvalidOperationException("Status context is not initialized.");
                }

                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(DisposableStatus));
                }

                return _context;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _completion.SetResult();
            if (_task is not null)
            {
                await _task;
            }

            _disposed = true;
        }
    }
}
