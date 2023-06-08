using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Json;

namespace Confix.Tool;

public static class ConsoleExtensions
{
    public static void Error(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[red]{message.EscapeMarkup()}[/]");
    }

    public static void Success(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[green]{message.EscapeMarkup()}[/]");
    }

    /// <summary>
    /// Writes a json object to the console
    /// </summary>
    /// <param name="console">
    /// The console to write to
    /// </param>
    /// <param name="data">
    /// The data to write
    /// </param>
    public static void WriteJson(this IAnsiConsole console, object? data)
    {
        var serializedData =
            JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true });

        var jsonText = new JsonText(serializedData);

        console.Write(jsonText);
    }

    public static ISpinner Spinner(this IAnsiConsole console, string message)
        => DefaultSpinner.Start(console, message);
}

file sealed class DefaultSpinner : ISpinner
{
    private readonly Task _completionTask;
    private readonly TaskCompletionSource _tcs;
    private readonly ContextAccessor _context;

    private DefaultSpinner(ContextAccessor context, TaskCompletionSource tcs, Task completionTask)
    {
        _context = context;
        _tcs = tcs;
        _completionTask = completionTask;
    }

    /// <inheritdoc />
    public string Message
    {
        get => _context.Context!.Status;
        set => _context.Context!.Status = value;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _tcs.SetResult();
        await _completionTask;
    }

    private sealed class ContextAccessor
    {
        public StatusContext? Context { get; set; }
    }

    public static DefaultSpinner Start(IAnsiConsole console, string message)
    {
        var tcs = new TaskCompletionSource();
        var context = new ContextAccessor();

        async Task StatAsync(StatusContext ctx)
        {
            context.Context = ctx;
            await tcs.Task;
        }

        var completionTask = console
            .Status()
            .StartAsync(message, StatAsync);

        return new DefaultSpinner(context, tcs, completionTask);
    }
}
