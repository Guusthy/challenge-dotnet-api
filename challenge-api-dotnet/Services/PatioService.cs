using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Services;

public sealed class PatioService(ApplicationDbContext db) : IPatioService
{
    private readonly ApplicationDbContext _db = db;

    private static (int page, int size) Normalize(int page, int size)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;
        return (page, size);
    }

    public async Task<PagedResult<PatioDTO>> GetPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.Patios.AsNoTracking();
        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(p => p.IdPatio)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(PatioMapper.ToDto).ToList();
        return new PagedResult<PatioDTO>(dtos, page, size, total);
    }

    public async Task<PatioDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.Patios.FindAsync(id);
        return entity is null ? null : PatioMapper.ToDto(entity);
    }

    public async Task<PagedResult<PatioDTO>> GetWithRelationsPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.Patios
            .AsNoTracking()
            .Where(p => p.Usuarios.Any() || p.Posicoes.Any() || p.MarcadoresFixos.Any());

        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(p => p.IdPatio)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(PatioMapper.ToDto).ToList();
        return new PagedResult<PatioDTO>(dtos, page, size, total);
    }

    public async Task<List<MotoDTO>> GetMotosByPatioAsync(int patioId)
    {
        var motos = await _db.Posicoes
            .AsNoTracking()
            .Where(p => p.PatioIdPatio == patioId && p.MotoIdMoto != null)
            .Include(p => p.MotoIdMotoNavigation)
            .Select(p => p.MotoIdMotoNavigation!)
            .Distinct()
            .ToListAsync();

        return motos.Select(MotoMapper.ToDto).ToList();
    }

    public async Task<PatioDTO> CreateAsync(PatioDTO dto)
    {
        var entity = PatioMapper.ToEntity(dto);
        _db.Patios.Add(entity);
        await _db.SaveChangesAsync();
        return PatioMapper.ToDto(entity);
    }

    public async Task<PatioDTO?> UpdateAsync(int id, PatioDTO dto)
    {
        var entity = await _db.Patios.FindAsync(id);
        if (entity is null) return null;

        entity.Nome = dto.Nome;
        entity.Localizacao = dto.Localizacao;
        entity.Descricao = dto.Descricao;

        await _db.SaveChangesAsync();
        return PatioMapper.ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Patios.FindAsync(id);
        if (entity is null) return false;

        _db.Patios.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}