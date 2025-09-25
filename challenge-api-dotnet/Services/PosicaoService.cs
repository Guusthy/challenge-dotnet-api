using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Services;

public sealed class PosicaoService(ApplicationDbContext db) : IPosicaoService
{
    private readonly ApplicationDbContext _db = db;

    private static (int page, int size) Normalize(int page, int size)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;
        return (page, size);
    }

    public async Task<PagedResult<PosicaoDTO>> GetPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.Posicoes.AsNoTracking();
        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(p => p.IdPosicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(PosicaoMapper.ToDto).ToList();
        return new PagedResult<PosicaoDTO>(dtos, page, size, total);
    }

    public async Task<PosicaoDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.Posicoes.FindAsync(id);
        return entity is null ? null : PosicaoMapper.ToDto(entity);
    }

    public async Task<PagedResult<PosicaoDTO>> GetByMotoIdPagedAsync(int motoId, int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.Posicoes.AsNoTracking().Where(p => p.MotoIdMoto == motoId);
        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(p => p.IdPosicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(PosicaoMapper.ToDto).ToList();
        return new PagedResult<PosicaoDTO>(dtos, page, size, total);
    }

    public async Task<PagedResult<PosicaoDTO>> GetHistoricoByMotoPagedAsync(int motoId, int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.Posicoes.AsNoTracking().Where(p => p.MotoIdMoto == motoId);
        var total = await query.LongCountAsync();

        var list = await query
            .OrderByDescending(p => p.DataHora)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(PosicaoMapper.ToDto).ToList();
        return new PagedResult<PosicaoDTO>(dtos, page, size, total);
    }

    public async Task<PagedResult<PosicaoDTO>> GetPosicoesDeMotosRevisaoPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.Posicoes
            .AsNoTracking()
            .Include(p => p.MotoIdMotoNavigation)
            .Where(p => p.MotoIdMotoNavigation != null &&
                        p.MotoIdMotoNavigation.Status.ToLower() == "revisão");

        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(p => p.IdPosicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(PosicaoMapper.ToDto).ToList();
        return new PagedResult<PosicaoDTO>(dtos, page, size, total);
    }

    public async Task<PosicaoDTO> CreateAsync(PosicaoDTO dto)
    {
        var entity = PosicaoMapper.ToEntity(dto);
        _db.Posicoes.Add(entity);
        await _db.SaveChangesAsync();
        return PosicaoMapper.ToDto(entity);
    }

    public async Task<PosicaoDTO?> UpdateAsync(int id, PosicaoDTO dto)
    {
        var entity = await _db.Posicoes.FindAsync(id);
        if (entity is null) return null;

        entity.XPos = dto.XPos;
        entity.YPos = dto.YPos;
        entity.DataHora = dto.DataHora;
        entity.MotoIdMoto = dto.MotoId;
        entity.PatioIdPatio = dto.PatioId;

        await _db.SaveChangesAsync();
        return PosicaoMapper.ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Posicoes.FindAsync(id);
        if (entity is null) return false;

        _db.Posicoes.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}