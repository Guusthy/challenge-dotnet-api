using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;

namespace challenge_api_dotnet.Services.Interfaces;

public interface IMarcadorFixoService
{
    Task<PagedResult<MarcadorFixoDTO>> GetPagedAsync(int page, int size);
    
    Task<MarcadorFixoDTO?> GetByIdAsync(int id);
    
    Task<PagedResult<MarcadorFixoDTO>> GetByPatioIdPagedAsync(int patioId, int page, int size);
    
    Task<MarcadorFixoDTO?> GetByCodigoArucoAsync(string codigoAruco);

    Task<MarcadorFixoDTO> CreateAsync(MarcadorFixoDTO dto);
    
    Task<bool> DeleteAsync(int id);
}