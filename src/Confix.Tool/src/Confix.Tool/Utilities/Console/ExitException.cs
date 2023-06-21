namespace Confix.Tool;

internal sealed class ExitException : Exception
{
    public ExitException() : base("")
    {
    }

    public ExitException(string message) : base(message)
    {
    }

    public ExitException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
