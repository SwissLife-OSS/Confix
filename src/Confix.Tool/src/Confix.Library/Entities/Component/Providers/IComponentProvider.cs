namespace Confix.Tool.Entities.Components;

public interface IComponentProvider
{
    public static virtual string Type => throw new NotImplementedException();

    Task ExecuteAsync(IComponentProviderContext context);
}
