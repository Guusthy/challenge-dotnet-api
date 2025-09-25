using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;

namespace challenge_api_dotnet.Services.Interfaces;

public interface IPatioService
{
    Task<PagedResult<PatioDTO>> GetPagedAsync(int page, int size);
    
    Task<PatioDTO?> GetByIdAsync(int id);

    // pátios que possuem usuários, posições OU marcadores fixos vinculados
    Task<PagedResult<PatioDTO>> GetWithRelationsPagedAsync(int page, int size);

    Task<List<MotoDTO>> GetMotosByPatioAsync(int patioId);

    Task<PatioDTO> CreateAsync(PatioDTO dto);
    
    Task<PatioDTO?> UpdateAsync(int id, PatioDTO dto);
    
    Task<bool> DeleteAsync(int id);
}