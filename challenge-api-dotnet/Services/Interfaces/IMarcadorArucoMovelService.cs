using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;

namespace challenge_api_dotnet.Services.Interfaces;

public interface IMarcadorArucoMovelService
{
    Task<PagedResult<MarcadorArucoMovelDTO>> GetPagedAsync(int page, int size);
    Task<MarcadorArucoMovelDTO?> GetByIdAsync(int id);
    Task<MarcadorArucoMovelDTO?> GetByMotoIdAsync(int motoId);
    Task<MarcadorArucoMovelDTO?> GetByCodigoArucoAsync(string codigoAruco);

    Task<MarcadorArucoMovelDTO> CreateAsync(MarcadorArucoMovelDTO dto);
    Task<MarcadorArucoMovelDTO?> UpdateAsync(int id, MarcadorArucoMovelDTO dto);
    Task<bool> DeleteAsync(int id);
}