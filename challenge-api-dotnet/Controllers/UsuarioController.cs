using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/usuario")]
[Produces("application/json")]
[Tags("Usuários")]
public class UsuarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public UsuarioController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar usuários")]
    [EndpointDescription("Retorna todos os usuários cadastrados.")]
    [ProducesResponseType(typeof(List<UsuarioResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsuarioResponseDTO>>> GetAll()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        return usuarios.Select(UsuarioMapper.ToResponseDto).ToList();
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter usuário por ID")]
    [EndpointDescription("Retorna os dados do usuário especificado pelo identificador.")]
    [ProducesResponseType(typeof(UsuarioResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponseDTO>> GetById([FromRoute] int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();
        return UsuarioMapper.ToResponseDto(usuario);
    }

    [HttpGet("email/{email}")]
    [EndpointSummary("Obter usuário por e-mail")]
    [EndpointDescription("Retorna o usuário correspondente ao e-mail informado.")]
    [ProducesResponseType(typeof(UsuarioResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponseDTO>> GetByEmail([FromRoute] string email)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null) return NotFound();
        return UsuarioMapper.ToResponseDto(usuario);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar usuário")]
    [EndpointDescription("Cria um novo usuário.")]
    [ProducesResponseType(typeof(UsuarioResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioResponseDTO>> Create([FromBody] UsuarioCreateDTO dto)
    {
        var usuario = UsuarioMapper.ToEntity(dto);
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var response = UsuarioMapper.ToResponseDto(usuario);
        return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario }, response);
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar usuário")]
    [EndpointDescription("Atualiza os dados de um usuário existente.")]
    [ProducesResponseType(typeof(UsuarioResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponseDTO>> Update([FromRoute] int id, [FromBody] UsuarioCreateDTO dto)
    {
        if (id != dto.IdUsuario) return BadRequest();

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        usuario.Nome = dto.Nome;
        usuario.Email = dto.Email;
        usuario.Senha = dto.Senha;
        usuario.Status = dto.Status;
        usuario.PatioIdPatio = dto.PatioId;

        await _context.SaveChangesAsync();
        return Ok(UsuarioMapper.ToResponseDto(usuario));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir usuário")]
    [EndpointDescription("Remove um usuário do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}