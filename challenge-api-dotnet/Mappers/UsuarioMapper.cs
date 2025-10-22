using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Models;

namespace challenge_api_dotnet.Mappers;

public class UsuarioMapper
{
    public static Usuario ToEntity(UsuarioCreateDTO dto) => new Usuario
    {
        IdUsuario = dto.IdUsuario,
        Nome = dto.Nome,
        Email = dto.Email,
        Senha = dto.Senha,
        Status = dto.Status,
        Tipo = dto.Tipo,
        PatioIdPatio = dto.PatioId
    };

    public static UsuarioResponseDTO ToResponseDto(Usuario usuario) => new UsuarioResponseDTO
    {
        IdUsuario = usuario.IdUsuario,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Status = usuario.Status,
        Tipo = usuario.Tipo,
        PatioId = usuario.PatioIdPatio

    };
}
