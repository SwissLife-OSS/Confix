using Confix.Tool.Commands.Project;
using Confix.Tool.Pipelines.Encryption;

namespace Confix.Tool.Commands.Encryption;

public sealed class FileEncryptCommand : PipelineCommand<FileEncryptPipeline>
{
    public FileEncryptCommand() : base("encrypt")
    {
        Description = "Encrypts a file using the configured provider";
    }
}
