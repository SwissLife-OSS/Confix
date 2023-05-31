namespace Confix.Tool.Common.Pipelines;

public interface IParameterCollection
{
    public T Get<T>(string key);

    public bool TryGet<T>(string key, out T value);
}
