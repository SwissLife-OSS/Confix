using System.CommandLine.Builder;
using System.Runtime.CompilerServices;

namespace Confix.Tool;

public static partial class CommandLineBuilderExtensions
{
    private static readonly ConditionalWeakTable<CommandLineBuilder, IDictionary<string, object>>
        _contextData = new();

    public static IDictionary<string, object> GetContextData(this CommandLineBuilder builder)
    {
        return _contextData.GetOrCreateValue(builder);
    }
}
