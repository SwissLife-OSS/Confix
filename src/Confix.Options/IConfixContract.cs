namespace Confix;

public interface IConfixContract
{
    Type OptionsType { get; }

    string Section { get; }

    string Name { get; }

    bool Required { get; }

    void Validate(IServiceProvider services);
}

