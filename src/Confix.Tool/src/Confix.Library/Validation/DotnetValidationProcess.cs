using System.Diagnostics;

namespace Confix.Tool.Validation;

internal static class DotnetValidationProcess
{
    public static async Task<(int ExitCode, string Output)> RunAsync(
        IEnumerable<string> arguments,
        string? input,
        string directory,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(5));

        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        // Build servers can keep redirected pipes open after the command has exited.
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0";
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new ExitException("Could not start dotnet.");
        using var registration = timeout.Token.Register(() => Stop(process));
        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        if (input is not null)
        {
            await process.StandardInput.WriteAsync(input.AsMemory(), timeout.Token);
        }
        process.StandardInput.Close();

        await process.WaitForExitAsync(timeout.Token);
        // Drain diagnostic output without exposing application values to the CLI log.
        await stderr;
        return (process.ExitCode, await stdout);
    }

    private static void Stop(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // The process has already exited.
        }
    }
}
