using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;

namespace challenge_api_dotnet.Services.Interfaces;

public interface IPosicaoService
{
    Task<PagedResult<PosicaoDTO>> GetPagedAsync(int page, int size);

    Task<PosicaoDTO?> GetByIdAsync(int id);

    Task<PagedResult<PosicaoDTO>> GetByMotoIdPagedAsync(int motoId, int page, int size);

    Task<PagedResult<PosicaoDTO>> GetHistoricoByMotoPagedAsync(int motoId, int page, int size);

    Task<PagedResult<PosicaoDTO>> GetPosicoesDeMotosRevisaoPagedAsync(int page, int size);

    Task<PosicaoDTO> CreateAsync(PosicaoDTO dto);

    Task<PosicaoDTO?> UpdateAsync(int id, PosicaoDTO dto);

    Task<bool> DeleteAsync(int id);
}