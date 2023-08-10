using Common.Components.DataProtection;
using Common.Components.Security;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSecurity();
builder.Services.AddDataProtectionFromConfig();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
