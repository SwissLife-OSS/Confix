using System.Diagnostics;
using System.Reflection;

namespace Confix.Runner;

/// <summary>
/// Builds the application's real host by invoking its entry point and capturing the
/// <c>HostBuilt</c> hosting diagnostic, then aborting before the host runs. This is the same
/// design-time mechanism used by Entity Framework tooling and WebApplicationFactory, so the
/// contracts examined are exactly the ones the application registers.
/// </summary>
internal sealed class HostCapture :
    IObserver<DiagnosticListener>,
    IObserver<KeyValuePair<string, object?>>
{
    private const string ListenerName = "Microsoft.Extensions.Hosting";
    private const string HostBuiltEvent = "HostBuilt";

    private static readonly AsyncLocal<HostCapture?> _current = new();

    private readonly MethodInfo _entryPoint;
    private readonly TaskCompletionSource<object> _host = new();
    private IDisposable? _subscription;

    private HostCapture(MethodInfo entryPoint)
    {
        _entryPoint = entryPoint;
    }

    public static CapturedHost Run(Assembly application, TimeSpan timeout)
    {
        var entryPoint = application.EntryPoint
            ?? throw new RunnerException("The application has no entry point.");

        var capture = new HostCapture(entryPoint);

        using var all = DiagnosticListener.AllListeners.Subscribe(capture);

        var completed = new TaskCompletionSource<Exception?>();

        var thread = new Thread(() =>
        {
            _current.Value = capture;

            try
            {
                var parameters = entryPoint.GetParameters().Length == 0
                    ? []
                    : new object[] { Array.Empty<string>() };

                entryPoint.Invoke(null, parameters);

                completed.TrySetResult(null);
            }
            catch (TargetInvocationException tie)
                when (tie.InnerException?.GetType().Name == "HostAbortedException")
            {
                // The host was stopped by the capture itself.
                completed.TrySetResult(null);
            }
            catch (TargetInvocationException tie)
            {
                completed.TrySetResult(tie.InnerException ?? tie);
            }
            catch (Exception ex)
            {
                completed.TrySetResult(ex);
            }
        })
        {
            IsBackground = true
        };

        thread.Start();

        var index = Task.WaitAny([capture._host.Task, completed.Task], timeout);

        if (index == 0)
        {
            return new CapturedHost(capture._host.Task.Result);
        }

        if (index == 1)
        {
            var failure = completed.Task.Result;

            throw failure is null
                ? new RunnerException(
                    "The application exited without building a host. Code-first validation " +
                    "requires Microsoft.Extensions.Hosting (Host or WebApplication builders).")
                : new RunnerException(
                    "The application failed while composing its host: " +
                    $"{failure.GetType().Name}. Guard code that cannot run at validation time " +
                    "with the CONFIX_VALIDATION environment variable.");
        }

        throw new RunnerException("Timed out waiting for the application to build its host.");
    }

    void IObserver<DiagnosticListener>.OnNext(DiagnosticListener listener)
    {
        if (_current.Value == this && listener.Name == ListenerName)
        {
            _subscription = listener.Subscribe(this);
        }
    }

    void IObserver<KeyValuePair<string, object?>>.OnNext(KeyValuePair<string, object?> value)
    {
        if (_current.Value != this || value.Key != HostBuiltEvent || value.Value is null)
        {
            return;
        }

        _host.TrySetResult(value.Value);

        ThrowHostAborted();
    }

    /// <summary>The public exception type ships in Hosting 7+; older hosts get a stand-in.</summary>
    private static void ThrowHostAborted()
    {
        var type = Type.GetType(
            "Microsoft.Extensions.Hosting.HostAbortedException, Microsoft.Extensions.Hosting.Abstractions",
            throwOnError: false);

        throw type is not null
            ? (Exception)Activator.CreateInstance(type)!
            : new HostAbortedFallbackException();
    }

    void IObserver<DiagnosticListener>.OnCompleted() => _subscription?.Dispose();

    void IObserver<DiagnosticListener>.OnError(Exception error)
    {
    }

    void IObserver<KeyValuePair<string, object?>>.OnCompleted()
    {
    }

    void IObserver<KeyValuePair<string, object?>>.OnError(Exception error)
    {
    }

    private sealed class HostAbortedFallbackException : Exception
    {
        public override string ToString() => "HostAbortedException";
    }
}

/// <summary>Accesses the captured host reflectively so runner and app hosting versions stay independent.</summary>
internal sealed class CapturedHost : IDisposable
{
    private readonly object _host;

    public CapturedHost(object host)
    {
        _host = host;
    }

    public IServiceProvider Services =>
        _host.GetType().GetProperty("Services")?.GetValue(_host) as IServiceProvider
            ?? throw new RunnerException("The captured host exposes no service provider.");

    public void Dispose() => (_host as IDisposable)?.Dispose();
}
