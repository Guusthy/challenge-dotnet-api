using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;

namespace challenge_api_dotnet.Services.Interfaces;

public interface IMedicaoPosicaoService
{
    Task<PagedResult<MedicaoPosicaoDTO>> GetPagedAsync(int page, int size);

    Task<MedicaoPosicaoDTO?> GetByIdAsync(int id);

    Task<PagedResult<MedicaoPosicaoDTO>> GetByPosicaoIdPagedAsync(int posicaoId, int page, int size);

    Task<PagedResult<MedicaoPosicaoDTO>> GetByMarcadorIdPagedAsync(int marcadorFixoId, int page, int size);

    Task<int> CountByPosicaoIdAsync(int posicaoId);

    Task<MedicaoPosicaoDTO> CreateAsync(MedicaoPosicaoDTO dto);
    
    Task<MedicaoPosicaoPredictionResponseDTO> PredictDistanceAsync(MedicaoPosicaoPredictionRequestDTO request);
}
