using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Services;

public sealed class MarcadorArucoMovelService(ApplicationDbContext db) : IMarcadorArucoMovelService
{
    private readonly ApplicationDbContext _db = db;

    // Trata dados da paginação
    private static (int page, int size) Normalize(int page, int size)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;
        return (page, size);
    }

    public async Task<PagedResult<MarcadorArucoMovelDTO>> GetPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        IQueryable<MarcadorArucoMovel> query = _db.MarcadoresArucoMoveis.AsNoTracking();
        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMarcadorMovel)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MarcadorArucoMovelMapper.ToDto).ToList();

        return new PagedResult<MarcadorArucoMovelDTO>(dtos, page, size, total);
    }

    public async Task<MarcadorArucoMovelDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.MarcadoresArucoMoveis.FindAsync(id);
        return entity is null ? null : MarcadorArucoMovelMapper.ToDto(entity);
    }

    public async Task<MarcadorArucoMovelDTO?> GetByMotoIdAsync(int motoId)
    {
        var entity = await _db.MarcadoresArucoMoveis.AsNoTracking()
            .FirstOrDefaultAsync(m => m.MotoIdMoto == motoId);
        return entity is null ? null : MarcadorArucoMovelMapper.ToDto(entity);
    }

    public async Task<MarcadorArucoMovelDTO?> GetByCodigoArucoAsync(string codigoAruco)
    {
        var code = codigoAruco.ToLower();
        var entity = await _db.MarcadoresArucoMoveis.AsNoTracking()
            .FirstOrDefaultAsync(m => m.CodigoAruco.ToLower() == code);
        return entity is null ? null : MarcadorArucoMovelMapper.ToDto(entity);
    }

    public async Task<MarcadorArucoMovelDTO> CreateAsync(MarcadorArucoMovelDTO dto)
    {
        var entity = MarcadorArucoMovelMapper.ToEntity(dto);
        _db.MarcadoresArucoMoveis.Add(entity);
        await _db.SaveChangesAsync();
        return MarcadorArucoMovelMapper.ToDto(entity);
    }

    public async Task<MarcadorArucoMovelDTO?> UpdateAsync(int id, MarcadorArucoMovelDTO dto)
    {
        var entity = await _db.MarcadoresArucoMoveis.FindAsync(id);
        if (entity is null) return null;

        entity.CodigoAruco = dto.CodigoAruco;
        entity.DataInstalacao = dto.DataInstalacao;
        entity.MotoIdMoto = dto.MotoId;

        await _db.SaveChangesAsync();
        return MarcadorArucoMovelMapper.ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.MarcadoresArucoMoveis.FindAsync(id);
        if (entity is null) return false;

        _db.MarcadoresArucoMoveis.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}