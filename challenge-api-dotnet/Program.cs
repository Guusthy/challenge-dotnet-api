using System;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using challenge_api_dotnet.Configs;
using challenge_api_dotnet.Data;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("x-api-version"),
            new QueryStringApiVersionReader("api-version"));
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddHealthChecks();
var jwtSection = builder.Configuration.GetSection("Jwt");
var privateKey = jwtSection["PrivateKey"] ?? throw new InvalidOperationException("Jwt:PrivateKey não está configurado.");
var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer não está configurado.");
var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience não está configurado.");

Configuration.SetJwtOptions(privateKey, issuer, audience);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

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

var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwaggerUI(c =>
{
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        c.SwaggerEndpoint($"/openapi/{description.GroupName}.json", $"Challenge API {description.ApiVersion}");
    }
    c.DocumentTitle = "Challenge API - Swagger UI";
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
