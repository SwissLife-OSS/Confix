using Confix.Tool.Common.Pipelines;

namespace ConfiX.Entities.Component.Configuration.Middlewares;

public class TestStatus : IStatus
{
    /// <inheritdoc />
    public string Status { get; set; } = string.Empty;
}
