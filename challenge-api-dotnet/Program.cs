using System;
using System.Text;
using challenge_api_dotnet.Configs;
using challenge_api_dotnet.Data;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Services;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
var jwtSection = builder.Configuration.GetSection("Jwt");
var privateKey = jwtSection["PrivateKey"] ?? throw new InvalidOperationException("Jwt:PrivateKey não está configurado.");
var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer não está configurado.");
var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience não está configurado.");

Configuration.SetJwtOptions(privateKey, issuer, audience);

builder.Services.AddScoped<ITokenService, TokenService>();

var signingKeyBytes = Encoding.UTF8.GetBytes(Configuration.PrivateKey);
if (signingKeyBytes.Length < 16)
{
    throw new InvalidOperationException("Jwt:PrivateKey deve possuir ao menos 128 bits.");
}

var signingKey = new SymmetricSecurityKey(signingKeyBytes);
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Configuration.Issuer,
            ValidateAudience = true,
            ValidAudience = Configuration.Audience,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => { options.UseOracle(connectionString); });

builder.Services.AddScoped<IMarcadorArucoMovelService, MarcadorArucoMovelService>();
builder.Services.AddScoped<IMarcadorFixoService, MarcadorFixoService>();
builder.Services.AddScoped<IMedicaoPosicaoService, MedicaoPosicaoService>();
builder.Services.AddScoped<IMotoService, MotoService>();
builder.Services.AddScoped<IPatioService, PatioService>();
builder.Services.AddScoped<IPosicaoService, PosicaoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();


var app = builder.Build();

app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}.json"; });

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/openapi/v1.json", "Challenge API v1");
    c.DocumentTitle = "Challenge API - Swagger UI";
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
