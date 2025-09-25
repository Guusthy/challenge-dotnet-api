using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;

namespace challenge_api_dotnet.Services.Interfaces;

public interface IMotoService
{
    Task<PagedResult<MotoDTO>> GetPagedAsync(int page, int size);

    Task<MotoDTO?> GetByIdAsync(int id);

    Task<PagedResult<MotoDTO>> GetByPlacaPagedAsync(string placa, int page, int size);

    Task<PagedResult<MotoDTO>> GetByStatusPagedAsync(string status, int page, int size);

    Task<List<PosicaoDTO>> GetPosicoesByMotoAsync(int motoId);

    Task<MotoDTO> CreateAsync(MotoCreateDTO dto);

    Task<MotoDTO?> UpdateAsync(int id, MotoCreateDTO dto);

    Task<bool> DeleteAsync(int id);
}