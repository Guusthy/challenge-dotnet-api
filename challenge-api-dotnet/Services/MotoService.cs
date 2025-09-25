using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Services;

public sealed class MotoService(ApplicationDbContext db) : IMotoService
{
    private readonly ApplicationDbContext _db = db;

    private static (int page, int size) Normalize(int page, int size)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;
        return (page, size);
    }

    public async Task<PagedResult<MotoDTO>> GetPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.Motos.AsNoTracking();
        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMoto)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MotoMapper.ToDto).ToList();
        return new PagedResult<MotoDTO>(dtos, page, size, total);
    }

    public async Task<MotoDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.Motos.FindAsync(id);
        return entity is null ? null : MotoMapper.ToDto(entity);
    }

    public async Task<PagedResult<MotoDTO>> GetByPlacaPagedAsync(string placa, int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.Motos
            .AsNoTracking()
            .Where(m => m.Placa.StartsWith(placa));

        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMoto)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MotoMapper.ToDto).ToList();
        return new PagedResult<MotoDTO>(dtos, page, size, total);
    }

    public async Task<PagedResult<MotoDTO>> GetByStatusPagedAsync(string status, int page, int size)
    {
        (page, size) = Normalize(page, size);

        var statusLower = status.ToLower();
        var query = _db.Motos
            .AsNoTracking()
            .Where(m => m.Status.ToLower() == statusLower);

        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMoto)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MotoMapper.ToDto).ToList();
        return new PagedResult<MotoDTO>(dtos, page, size, total);
    }

    public async Task<List<PosicaoDTO>> GetPosicoesByMotoAsync(int motoId)
    {
        var list = await _db.Posicoes
            .AsNoTracking()
            .Where(p => p.MotoIdMoto == motoId)
            .ToListAsync();

        return list.Select(PosicaoMapper.ToDto).ToList();
    }

    public async Task<MotoDTO> CreateAsync(MotoCreateDTO dto)
    {
        var entity = MotoMapper.ToEntity(dto);
        entity.DataCadastro = DateTime.Now;

        _db.Motos.Add(entity);
        await _db.SaveChangesAsync();

        return MotoMapper.ToDto(entity);
    }

    public async Task<MotoDTO?> UpdateAsync(int id, MotoCreateDTO dto)
    {
        var entity = await _db.Motos.FindAsync(id);
        if (entity is null) return null;

        entity.Placa = dto.Placa;
        entity.Modelo = dto.Modelo;
        entity.Status = dto.Status;

        await _db.SaveChangesAsync();
        return MotoMapper.ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Motos.FindAsync(id);
        if (entity is null) return false;

        _db.Motos.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}