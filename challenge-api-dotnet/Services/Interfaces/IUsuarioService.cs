using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;

namespace challenge_api_dotnet.Services.Interfaces;

public interface IUsuarioService
{
    Task<PagedResult<UsuarioResponseDTO>> GetPagedAsync(int page, int size);

    Task<UsuarioResponseDTO?> GetByIdAsync(int id);

    Task<UsuarioResponseDTO?> GetByEmailAsync(string email);

    Task<UsuarioResponseDTO> CreateAsync(UsuarioCreateDTO dto);

    Task<UsuarioResponseDTO?> UpdateAsync(int id, UsuarioCreateDTO dto);

    Task<bool> DeleteAsync(int id);
}