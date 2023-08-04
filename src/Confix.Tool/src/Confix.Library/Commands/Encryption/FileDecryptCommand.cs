using Confix.Tool.Commands.Project;
using Confix.Tool.Pipelines.Encryption;

namespace Confix.Tool.Commands.Encryption;

public sealed class FileDecryptCommand : PipelineCommand<FileDecryptPipeline>
{
    public FileDecryptCommand() : base("decrypt")
    {
        Description = "Decrypts a file using the configured provider";
    }
}
