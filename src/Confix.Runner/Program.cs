using Confix.Runner;

// stdout is exclusively the protocol. Application callbacks cannot corrupt it or print secrets.
var output = Console.Out;
Console.SetOut(TextWriter.Null);
Console.SetError(TextWriter.Null);

var request = await Console.In.ReadToEndAsync();

return await ValidationRunner.RunAsync(args, request, output);
