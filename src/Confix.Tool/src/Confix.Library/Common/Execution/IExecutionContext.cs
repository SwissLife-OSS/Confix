namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// <para>
/// Represents the context of the current execution, providing information such as the current
/// directory and home directory. This interface is used to pass information about the execution
/// environment to middlewares, which can help avoid binding the middlewares to the command line.
/// </para>
/// <para>
/// The execution context is vital for many operations that require knowledge about the file
/// system's current state or user-specific data (like user's home directory).
/// </para>
/// </summary>
public interface IExecutionContext
{
    /// <summary>
    /// Gets the current directory of the execution context. 
    /// This is usually the directory where the application was called or started from.
    /// </summary>
    DirectoryInfo CurrentDirectory { get; }

    /// <summary>
    /// Gets the home directory of the current user. In a Windows environment, this could be
    /// something like "C:\Users\JohnDoe", while on macOS or Linux it might be "/Users/johndoe" or
    /// "/home/johndoe" respectively. The home directory is determined using
    /// <code>Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)</code>.
    /// </summary>   
    DirectoryInfo HomeDirectory { get; }
}
