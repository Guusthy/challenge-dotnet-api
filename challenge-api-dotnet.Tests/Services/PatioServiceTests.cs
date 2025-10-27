using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace challenge_api_dotnet.Tests.Services;

public sealed class PatioServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PatioService _service;

    public PatioServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        SeedData(_context);
        _service = new PatioService(_context);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsExpectedPagination()
    {
        var result = await _service.GetPagedAsync(page: 1, size: 2);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.Size);
        Assert.Equal(3, result.Total);
        Assert.Equal(2, result.Items.Count());
        Assert.Contains(result.Items, p => p.Nome == "Patio A");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        var patio = await _service.GetByIdAsync(2);

        Assert.NotNull(patio);
        Assert.Equal("Patio B", patio!.Nome);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        var patio = await _service.GetByIdAsync(999);

        Assert.Null(patio);
    }

    [Fact]
    public async Task GetWithRelationsPagedAsync_ReturnsOnlyPatiosWithRelations()
    {
        var result = await _service.GetWithRelationsPagedAsync(page: 1, size: 5);

        Assert.Equal(2, result.Total);
        Assert.All(result.Items, p => Assert.Contains(p.IdPatio, new[] { 1, 2 }));
    }

    [Fact]
    public async Task GetMotosByPatioAsync_ReturnsDistinctMotos()
    {
        var motos = await _service.GetMotosByPatioAsync(1);

        Assert.Equal(2, motos.Count);
        Assert.All(motos, m => Assert.Contains(m.IdMoto, new[] { 1, 2 }));
    }

    [Fact]
    public async Task CreateAsync_PersistsPatio()
    {
        var dto = new PatioDTO
        {
            Nome = "Novo Patio",
            Localizacao = "Rua Nova, 123",
            Descricao = "Patio recem-criado"
        };

        var created = await _service.CreateAsync(dto);

        Assert.NotEqual(0, created.IdPatio);
        var entity = await _context.Patios.FindAsync(created.IdPatio);
        Assert.NotNull(entity);
        Assert.Equal("Novo Patio", entity!.Nome);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesFields()
    {
        var dto = new PatioDTO
        {
            IdPatio = 1,
            Nome = "Patio Atualizado",
            Localizacao = "Localizacao Atualizada",
            Descricao = "Descricao Atualizada"
        };

        var updated = await _service.UpdateAsync(1, dto);

        Assert.NotNull(updated);
        Assert.Equal("Patio Atualizado", updated!.Nome);
        var entity = await _context.Patios.FindAsync(1);
        Assert.Equal("Descricao Atualizada", entity!.Descricao);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ReturnsNull()
    {
        var dto = new PatioDTO
        {
            IdPatio = 999,
            Nome = "Inexistente"
        };

        var updated = await _service.UpdateAsync(999, dto);

        Assert.Null(updated);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesPatio()
    {
        var removed = await _service.DeleteAsync(3);

        Assert.True(removed);
        Assert.Null(await _context.Patios.FindAsync(3));
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        var removed = await _service.DeleteAsync(999);

        Assert.False(removed);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static void SeedData(ApplicationDbContext context)
    {
        context.Patios.AddRange(
            new Patio { IdPatio = 1, Nome = "Patio A", Localizacao = "Rua 1", Descricao = "Principal" },
            new Patio { IdPatio = 2, Nome = "Patio B", Localizacao = "Rua 2", Descricao = "Secundario" },
            new Patio { IdPatio = 3, Nome = "Patio C", Localizacao = "Rua 3", Descricao = "Sem relacoes" }
        );

        context.Motos.AddRange(
            new Moto { IdMoto = 1, Placa = "AAA1111", Modelo = "Modelo 1", Status = "ativo", DataCadastro = DateTime.UtcNow.AddDays(-2) },
            new Moto { IdMoto = 2, Placa = "BBB2222", Modelo = "Modelo 2", Status = "ativo", DataCadastro = DateTime.UtcNow.AddDays(-1) },
            new Moto { IdMoto = 3, Placa = "CCC3333", Modelo = "Modelo 3", Status = "inativo", DataCadastro = DateTime.UtcNow }
        );

        context.Posicoes.AddRange(
            new Posicao { IdPosicao = 1, PatioIdPatio = 1, MotoIdMoto = 1, XPos = 1, YPos = 1 },
            new Posicao { IdPosicao = 2, PatioIdPatio = 1, MotoIdMoto = 2, XPos = 2, YPos = 2 },
            new Posicao { IdPosicao = 3, PatioIdPatio = 2, MotoIdMoto = 1, XPos = 3, YPos = 3 },
            new Posicao { IdPosicao = 4, PatioIdPatio = 2, MotoIdMoto = null, XPos = 4, YPos = 4 }
        );

        context.Usuarios.AddRange(
            new Usuario { IdUsuario = 1, Nome = "Ana", Email = "ana@example.com", Senha = "senha", PatioIdPatio = 1 },
            new Usuario { IdUsuario = 2, Nome = "Bruno", Email = "bruno@example.com", Senha = "senha", PatioIdPatio = 2 }
        );

        context.MarcadoresFixos.AddRange(
            new MarcadorFixo { IdMarcadorArucoFixo = 1, CodigoAruco = "MK1", PatioIdPatio = 1 },
            new MarcadorFixo { IdMarcadorArucoFixo = 2, CodigoAruco = "MK2", PatioIdPatio = 2 }
        );

        context.SaveChanges();
    }
}
