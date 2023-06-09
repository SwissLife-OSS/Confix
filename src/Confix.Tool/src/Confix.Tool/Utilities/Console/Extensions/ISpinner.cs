namespace Confix.Tool;

public interface ISpinner : IAsyncDisposable
{
    string Message { get; set; }
}
