using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Services;

public sealed class MedicaoPosicaoService(ApplicationDbContext db) : IMedicaoPosicaoService
{
    private readonly ApplicationDbContext _db = db;

    private static (int page, int size) Normalize(int page, int size)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;
        return (page, size);
    }

    public async Task<PagedResult<MedicaoPosicaoDTO>> GetPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.MedicoesPosicoes.AsNoTracking();
        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMedicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MedicaoPosicaoMapper.ToDto).ToList();
        return new PagedResult<MedicaoPosicaoDTO>(dtos, page, size, total);
    }

    public async Task<MedicaoPosicaoDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.MedicoesPosicoes.FindAsync(id);
        return entity is null ? null : MedicaoPosicaoMapper.ToDto(entity);
    }

    public async Task<PagedResult<MedicaoPosicaoDTO>> GetByPosicaoIdPagedAsync(int posicaoId, int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.MedicoesPosicoes
            .AsNoTracking()
            .Where(m => m.PosicaoIdPosicao == posicaoId);

        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMedicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MedicaoPosicaoMapper.ToDto).ToList();
        return new PagedResult<MedicaoPosicaoDTO>(dtos, page, size, total);
    }

    public async Task<PagedResult<MedicaoPosicaoDTO>> GetByMarcadorIdPagedAsync(int marcadorFixoId, int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.MedicoesPosicoes
            .AsNoTracking()
            .Where(m => m.MarcadorFixoIdMarcadorArucoFixo == marcadorFixoId);

        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMedicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MedicaoPosicaoMapper.ToDto).ToList();
        return new PagedResult<MedicaoPosicaoDTO>(dtos, page, size, total);
    }

    public async Task<int> CountByPosicaoIdAsync(int posicaoId)
    {
        return await _db.MedicoesPosicoes
            .AsNoTracking()
            .CountAsync(m => m.PosicaoIdPosicao == posicaoId);
    }

    public async Task<MedicaoPosicaoDTO> CreateAsync(MedicaoPosicaoDTO dto)
    {
        var entity = MedicaoPosicaoMapper.ToEntity(dto);
        _db.MedicoesPosicoes.Add(entity);
        await _db.SaveChangesAsync();
        return MedicaoPosicaoMapper.ToDto(entity);
    }
}