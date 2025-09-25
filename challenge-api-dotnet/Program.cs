using challenge_api_dotnet.Data;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Services;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Challenge API",
        Version = "v1",
        Description = "API do Challenge – gestão de motos/pátios."
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => { options.UseOracle(connectionString); });
builder.Services.AddScoped<IMarcadorArucoMovelService, MarcadorArucoMovelService>();
builder.Services.AddScoped<IMarcadorFixoService, MarcadorFixoService>();
builder.Services.AddScoped<IMedicaoPosicaoService, MedicaoPosicaoService>();
builder.Services.AddScoped<IMotoService, MotoService>();


var app = builder.Build();

app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}.json"; });

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/openapi/v1.json", "Challenge API v1");
    c.DocumentTitle = "Challenge API - Swagger UI";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();