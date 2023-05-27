using System.CommandLine.Parsing;
using Confix.Tool;

return await new ConfixCommandLine().Build().InvokeAsync(args);
