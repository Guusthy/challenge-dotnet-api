using Asp.Versioning;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace challenge_api_dotnet.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
[Tags("Autenticação")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;
    

    [HttpPost("register")]
    [Consumes("application/json")]
    [EndpointSummary("Registrar novo usuário")]
    [EndpointDescription("Cria um usuário e retorna os dados com token JWT.")]
    [ProducesResponseType(typeof(Resource<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<AuthResponseDto>>> Register([FromBody] UsuarioCreateDTO dto)
    {
        var (usuario, token) = await _authService.RegisterAsync(dto);

        var response = new AuthResponseDto(usuario, token);
        var links = new List<HateoasLink>
        {
            new("login", Url.ActionHref(nameof(Login)), "POST")
        };

        return CreatedAtAction(nameof(Login), new { email = usuario.Email }, new Resource<AuthResponseDto>(response, links));
    }

    [HttpPost("login")]
    [Consumes("application/json")]
    [EndpointSummary("Autenticar usuário")]
    [EndpointDescription("Validar credenciais e retornar token JWT.")]
    [ProducesResponseType(typeof(Resource<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Resource<AuthResponseDto>>> Login([FromBody] LoginRequestDto dto)
    {
        var (usuario, token) = await _authService.LoginAsync(dto.Email, dto.Senha);

        var response = new AuthResponseDto(usuario, token);
        var links = new List<HateoasLink>
        {
            new("register", Url.ActionHref(nameof(Register)), "POST")
        };

        return Ok(new Resource<AuthResponseDto>(response, links));
    }
}
