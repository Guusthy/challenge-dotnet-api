using challenge_api_dotnet.Models;

namespace challenge_api_dotnet.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(Usuario usuario);
}
