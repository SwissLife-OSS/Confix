using Database;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabase();
var app = builder.Build();

app.MapGet("/", ([FromServices] IConfiguration configuration) => configuration["Website:Url"]);

app.Run();
