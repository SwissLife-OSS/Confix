namespace Confix.Tool.Entities.Components;

public interface IComponentProvider
{
    public static virtual string Type => string.Empty;

    Task ExecuteAsync(IComponentProviderContext context);
}
