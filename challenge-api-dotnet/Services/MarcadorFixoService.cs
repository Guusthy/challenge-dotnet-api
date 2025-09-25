using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Services;

public sealed class MarcadorFixoService(ApplicationDbContext db) : IMarcadorFixoService
{
    private readonly ApplicationDbContext _db = db;

    private static (int page, int size) Normalize(int page, int size)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;
        return (page, size);
    }

    public async Task<PagedResult<MarcadorFixoDTO>> GetPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.MarcadoresFixos.AsNoTracking();
        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMarcadorArucoFixo)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MarcadorFixoMapper.ToDto).ToList();
        return new PagedResult<MarcadorFixoDTO>(dtos, page, size, total);
    }

    public async Task<MarcadorFixoDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.MarcadoresFixos.FindAsync(id);
        return entity is null ? null : MarcadorFixoMapper.ToDto(entity);
    }

    public async Task<PagedResult<MarcadorFixoDTO>> GetByPatioIdPagedAsync(int patioId, int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.MarcadoresFixos
            .AsNoTracking()
            .Where(m => m.PatioIdPatio == patioId);

        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMarcadorArucoFixo)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MarcadorFixoMapper.ToDto).ToList();
        return new PagedResult<MarcadorFixoDTO>(dtos, page, size, total);
    }

    public async Task<MarcadorFixoDTO?> GetByCodigoArucoAsync(string codigoAruco)
    {
        var code = codigoAruco.ToLower();
        var entity = await _db.MarcadoresFixos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CodigoAruco.ToLower() == code);

        return entity is null ? null : MarcadorFixoMapper.ToDto(entity);
    }

    public async Task<MarcadorFixoDTO> CreateAsync(MarcadorFixoDTO dto)
    {
        var entity = MarcadorFixoMapper.ToEntity(dto);
        _db.MarcadoresFixos.Add(entity);
        await _db.SaveChangesAsync();
        return MarcadorFixoMapper.ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.MarcadoresFixos.FindAsync(id);
        if (entity is null) return false;

        _db.MarcadoresFixos.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}