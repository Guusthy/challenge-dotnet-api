using challenge_api_dotnet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Challenge API",
            Version = "v1",
            Description = "API do Challenge – gestão de motos/pátios.",
        };
        return Task.CompletedTask;
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseOracle(connectionString);
});

var app = builder.Build();

app.MapOpenApi();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/openapi/v1.json", "Challenge API v1");
    c.DocumentTitle = "Challenge API - Swagger UI";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();