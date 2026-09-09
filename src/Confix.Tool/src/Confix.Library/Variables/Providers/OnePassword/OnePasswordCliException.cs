namespace Confix.Variables;

public sealed class OnePasswordCliException : Exception
{
    public int ExitCode { get; }

    public OnePasswordCliException(int exitCode, string message)
        : base(message)
    {
        ExitCode = exitCode;
    }
}
