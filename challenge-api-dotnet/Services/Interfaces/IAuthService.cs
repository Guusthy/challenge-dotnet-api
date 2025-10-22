using challenge_api_dotnet.Dtos;

namespace challenge_api_dotnet.Services.Interfaces;

public interface IAuthService
{
    Task<(UsuarioResponseDTO Usuario, string Token)> RegisterAsync(UsuarioCreateDTO dto);

    Task<(UsuarioResponseDTO Usuario, string Token)> LoginAsync(string email, string senha);
}
