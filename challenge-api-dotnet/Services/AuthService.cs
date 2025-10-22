using System;
using System.Threading.Tasks;
using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Services;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AuthService(ApplicationDbContext db, ITokenService tokenService, IPasswordHasher<Usuario> passwordHasher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<(UsuarioResponseDTO Usuario, string Token)> RegisterAsync(UsuarioCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ArgumentException("Email é obrigatório.", nameof(dto));
        }
        if (string.IsNullOrWhiteSpace(dto.Senha))
        {
            throw new ArgumentException("Senha é obrigatória.", nameof(dto));
        }

        var email = dto.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();
        var exists = await _db.Usuarios
            .CountAsync(u => u.Email.ToLower() == normalizedEmail) > 0;
        if (exists)
        {
            throw new InvalidOperationException("Email já cadastrado.");
        }
        
        dto.Email = email;
        
        var originalPassword = dto.Senha;

        var entity = UsuarioMapper.ToEntity(dto);
        entity.Senha = _passwordHasher.HashPassword(entity, originalPassword);
        _db.Usuarios.Add(entity);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(entity);
        var response = UsuarioMapper.ToResponseDto(entity);

        return (response, token);
    }

    public async Task<(UsuarioResponseDTO Usuario, string Token)> LoginAsync(string email, string senha)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email é obrigatório.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(senha))
        {
            throw new ArgumentException("Senha é obrigatória.", nameof(senha));
        }

        var trimmedEmail = email.Trim();
        var normalizedEmail = trimmedEmail.ToLowerInvariant();
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (usuario is null)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, senha);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.Senha = _passwordHasher.HashPassword(usuario, senha);
            await _db.SaveChangesAsync();
        }

        var token = _tokenService.GenerateToken(usuario);
        var response = UsuarioMapper.ToResponseDto(usuario);

        return (response, token);
    }
}
