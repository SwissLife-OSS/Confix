using Microsoft.Extensions.Options;
namespace Confix;

/// <summary>Contains only diagnostics constructed by Confix, never custom validator messages.</summary>
public sealed class ConfixValidationException : OptionsValidationException
{
    internal ConfixValidationException(string name, Type type, IEnumerable<string> failures) : base(name, type, failures) { }
}
