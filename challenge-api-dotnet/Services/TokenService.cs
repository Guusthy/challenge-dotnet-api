using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using challenge_api_dotnet.Configs;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace challenge_api_dotnet.Services;

public sealed class TokenService : ITokenService
{
    public string GenerateToken(Usuario usuario)
    {
        if (usuario is null)
        {
            throw new ArgumentNullException(nameof(usuario));
        }

        var handler = new JwtSecurityTokenHandler();
        var privateKey = Configuration.PrivateKey;
        if (string.IsNullOrWhiteSpace(privateKey))
        {
            throw new InvalidOperationException("JWT private key não inicializada.");
        }

        var key = Encoding.UTF8.GetBytes(privateKey);
        if (key.Length < 16)
        {
            throw new InvalidOperationException("JWT private key deve ter pelo menos 128 bits.");
        }

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);
        var identity = GenerateClaimsIdentity(usuario);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Issuer = Configuration.Issuer,
            Audience = Configuration.Audience,
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddHours(2)
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    private static ClaimsIdentity GenerateClaimsIdentity(Usuario usuario)
    {
        if (usuario is null)
        {
            throw new ArgumentNullException(nameof(usuario));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.Nome),
            new("patioId", usuario.PatioIdPatio.ToString())
        };

        if (!string.IsNullOrWhiteSpace(usuario.Status))
        {
            claims.Add(new Claim("status", usuario.Status));
        }

        if (!string.IsNullOrWhiteSpace(usuario.Tipo))
        {
            claims.Add(new Claim(ClaimTypes.Role, usuario.Tipo));
        }

        return new ClaimsIdentity(claims, "JWT");
    }
}
