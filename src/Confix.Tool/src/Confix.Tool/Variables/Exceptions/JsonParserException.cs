namespace Confix.Variables;

public sealed class JsonParserException : Exception
{
    public JsonParserException(string? message) : base(message)
    {
    }

    public JsonParserException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
