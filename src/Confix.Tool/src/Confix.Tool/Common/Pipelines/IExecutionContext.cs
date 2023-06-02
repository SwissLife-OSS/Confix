namespace Confix.Tool.Common.Pipelines;

public interface IExecutionContext
{
    string CurrentDirectory { get; }
    
    string HomeDirectory { get; }
}
