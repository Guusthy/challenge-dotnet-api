using System;
using System.Threading.Tasks;
using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace challenge_api_dotnet.Tests.Services;

public sealed class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _service;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        SeedData(_context, _passwordHasher);
        _service = new AuthService(_context, new FakeTokenService(), _passwordHasher);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_PersistsHashedPasswordAndReturnsToken()
    {
        var dto = new UsuarioCreateDTO
        {
            Nome = "Novo",
            Email = "novo@example.com",
            Senha = "Password123!",
            Status = "ativo",
            Tipo = "admin",
            PatioId = 1
        };

        var (usuario, token) = await _service.RegisterAsync(dto);

        Assert.Equal("novo@example.com", usuario.Email);
        Assert.False(string.IsNullOrWhiteSpace(token));
        var entity = await _context.Usuarios.SingleAsync(u => u.Email == "novo@example.com");
        Assert.NotEqual("Password123!", entity.Senha);
        Assert.Equal(PasswordVerificationResult.Success, _passwordHasher.VerifyHashedPassword(entity, entity.Senha, "Password123!"));
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ThrowsInvalidOperationException()
    {
        var dto = new UsuarioCreateDTO
        {
            Nome = "Duplicado",
            Email = "carlos@example.com",
            Senha = "Senha123!",
            Status = "ativo",
            Tipo = "admin",
            PatioId = 1
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var (usuario, token) = await _service.LoginAsync("carlos@example.com", "SenhaSegura!1");

        Assert.Equal("carlos@example.com", usuario.Email);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccess()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync("carlos@example.com", "SenhaErrada"));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static void SeedData(ApplicationDbContext context, IPasswordHasher<Usuario> passwordHasher)
    {
        context.Patios.Add(new Patio { IdPatio = 1, Nome = "Patio Teste" });

        var usuario = new Usuario
        {
            IdUsuario = 1,
            Nome = "Carlos",
            Email = "carlos@example.com",
            Senha = "temp",
            Status = "ativo",
            Tipo = "admin",
            PatioIdPatio = 1
        };

        usuario.Senha = passwordHasher.HashPassword(usuario, "SenhaSegura!1");
        context.Usuarios.Add(usuario);

        context.SaveChanges();
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenerateToken(Usuario usuario) => $"token-{usuario.Email}";
    }
}
