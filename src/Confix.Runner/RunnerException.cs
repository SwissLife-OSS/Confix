namespace Confix.Runner;

/// <summary>Carries a runner-authored message that is safe to surface to the CLI.</summary>
internal sealed class RunnerException(string message) : Exception(message);
