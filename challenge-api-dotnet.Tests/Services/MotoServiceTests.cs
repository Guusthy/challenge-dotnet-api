using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace challenge_api_dotnet.Tests.Services;

public class MotoServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly MotoService _service;

    public MotoServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        SeedData(_context);
        _service = new MotoService(_context);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsExpectedCountAndPagination()
    {
        var result = await _service.GetPagedAsync(page: 1, size: 2);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.Size);
        Assert.Equal(3, result.Total);
        Assert.Equal(2, result.Items.Count());
        Assert.Contains(result.Items, m => m.Placa == "ABC1234");
    }

    [Fact]
    public async Task GetByStatusPagedAsync_IsCaseInsensitive()
    {
        var result = await _service.GetByStatusPagedAsync("ATIVO", page: 1, size: 10);

        Assert.Equal(2, result.Total);
        Assert.All(result.Items, m => Assert.Equal("ativo", m.Status.ToLowerInvariant()));
    }

    [Fact]
    public async Task CreateAsync_PersistsMotoAndSetsDataCadastro()
    {
        var dto = new MotoCreateDTO
        {
            Placa = "JKL4567",
            Modelo = "Model X",
            Status = "ativo"
        };

        var created = await _service.CreateAsync(dto);

        Assert.NotEqual(0, created.IdMoto);
        var entity = await _context.Motos.FindAsync(created.IdMoto);
        Assert.NotNull(entity);
        Assert.NotEqual(default, entity!.DataCadastro);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesFields()
    {
        var dto = new MotoCreateDTO
        {
            Placa = "UPDATED1",
            Modelo = "Updated",
            Status = "inativo"
        };

        var updated = await _service.UpdateAsync(1, dto);

        Assert.NotNull(updated);
        Assert.Equal("UPDATED1", updated!.Placa);
        var entity = await _context.Motos.FindAsync(1);
        Assert.Equal("inativo", entity!.Status);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        var dto = new MotoCreateDTO { Placa = "Z", Modelo = "Z", Status = "ativo" };
        var updated = await _service.UpdateAsync(999, dto);
        Assert.Null(updated);
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingMoto()
    {
        var result = await _service.DeleteAsync(2);
        Assert.True(result);
        Assert.Null(await _context.Motos.FindAsync(2));
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(999);
        Assert.False(result);
    }

    [Fact]
    public async Task GetPosicoesByMotoAsync_ReturnsOnlyRelatedPositions()
    {
        var positions = await _service.GetPosicoesByMotoAsync(1);

        Assert.Equal(2, positions.Count);
        Assert.All(positions, p => Assert.Equal(1, p.MotoId));
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static void SeedData(ApplicationDbContext context)
    {
        context.Motos.AddRange(
            new Moto { IdMoto = 1, Placa = "ABC1234", Modelo = "CG 160", Status = "ativo", DataCadastro = DateTime.UtcNow.AddDays(-2) },
            new Moto { IdMoto = 2, Placa = "DEF5678", Modelo = "Factor 150", Status = "ativo", DataCadastro = DateTime.UtcNow.AddDays(-1) },
            new Moto { IdMoto = 3, Placa = "GHI9012", Modelo = "Biz 110", Status = "inativo", DataCadastro = DateTime.UtcNow }
        );

        context.Posicoes.AddRange(
            new Posicao { IdPosicao = 1, MotoIdMoto = 1, XPos = 1, YPos = 1 },
            new Posicao { IdPosicao = 2, MotoIdMoto = 1, XPos = 2, YPos = 2 },
            new Posicao { IdPosicao = 3, MotoIdMoto = 2, XPos = 3, YPos = 3 }
        );

        context.SaveChanges();
    }
}
