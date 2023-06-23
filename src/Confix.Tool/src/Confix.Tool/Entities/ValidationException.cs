namespace Confix.Tool;

internal sealed class ValidationException : Exception
{

    public ValidationException(string message)
        : base(message)
    {
    }

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}