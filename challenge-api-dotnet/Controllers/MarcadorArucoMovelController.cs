using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/marcador-movel")]
[Produces("application/json")]
[Tags("Marcadores ArUco Móveis")]
public class MarcadorArucoMovelController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public MarcadorArucoMovelController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar marcadores móveis")]
    [EndpointDescription("Retorna todos os marcadores ArUco móveis cadastrados.")]
    [ProducesResponseType(typeof(List<MarcadorArucoMovelDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MarcadorArucoMovelDTO>>> GetAll()
    {
        var marcadores = await _context.MarcadoresArucoMoveis.ToListAsync();
        return marcadores.Select(MarcadorArucoMovelMapper.ToDto).ToList();
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter marcador móvel por ID")]
    [EndpointDescription("Retorna os dados de um marcador ArUco móvel a partir do seu identificador.")]
    [ProducesResponseType(typeof(MarcadorArucoMovelDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MarcadorArucoMovelDTO>> GetById([FromRoute] int id)
    {
        var marcador = await _context.MarcadoresArucoMoveis.FindAsync(id);
        if (marcador == null) return NotFound();
        return MarcadorArucoMovelMapper.ToDto(marcador);
    }

    [HttpGet("moto/{idMoto}")]
    [EndpointSummary("Obter marcador por moto")]
    [EndpointDescription("Retorna o marcador ArUco móvel vinculado a uma moto específica.")]
    [ProducesResponseType(typeof(MarcadorArucoMovelDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MarcadorArucoMovelDTO>> GetByMotoId([FromRoute] int idMoto)
    {
        var marcador = await _context.MarcadoresArucoMoveis
            .FirstOrDefaultAsync(m => m.MotoIdMoto == idMoto);

        if (marcador == null) return NotFound();
        return MarcadorArucoMovelMapper.ToDto(marcador);
    }

    [HttpGet("busca")]
    [EndpointSummary("Buscar marcador por código ArUco")]
    [EndpointDescription("Retorna um marcador ArUco móvel filtrado pelo código passado na querystring. Ex: MOVEL_002")]
    [ProducesResponseType(typeof(MarcadorArucoMovelDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MarcadorArucoMovelDTO>> GetByCodigoAruco([FromQuery] string codigoAruco)
    {
        var marcador = await _context.MarcadoresArucoMoveis
            .FirstOrDefaultAsync(m => m.CodigoAruco.ToLower() == codigoAruco.ToLower());

        if (marcador == null) return NotFound();
        return MarcadorArucoMovelMapper.ToDto(marcador);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar marcador móvel")]
    [EndpointDescription("Cria um novo marcador ArUco móvel.")]
    [ProducesResponseType(typeof(MarcadorArucoMovelDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MarcadorArucoMovelDTO>> Create([FromBody] MarcadorArucoMovelDTO dto)
    {
        var marcador = MarcadorArucoMovelMapper.ToEntity(dto);
        _context.MarcadoresArucoMoveis.Add(marcador);
        await _context.SaveChangesAsync();

        var response = MarcadorArucoMovelMapper.ToDto(marcador);
        return CreatedAtAction(nameof(GetById), new { id = marcador.IdMarcadorMovel }, response);
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar marcador móvel")]
    [EndpointDescription("Atualiza os dados de um marcador ArUco móvel existente.")]
    [ProducesResponseType(typeof(MarcadorArucoMovelDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MarcadorArucoMovelDTO>> Update([FromRoute] int id, [FromBody] MarcadorArucoMovelDTO dto)
    {
        if (id != dto.IdMarcadorMovel) return BadRequest();

        var marcador = await _context.MarcadoresArucoMoveis.FindAsync(id);
        if (marcador == null) return NotFound();

        marcador.CodigoAruco = dto.CodigoAruco;
        marcador.DataInstalacao = dto.DataInstalacao;
        marcador.MotoIdMoto = dto.MotoId;

        await _context.SaveChangesAsync();
        return Ok(MarcadorArucoMovelMapper.ToDto(marcador));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir marcador móvel")]
    [EndpointDescription("Remove um marcador ArUco móvel do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var marcador = await _context.MarcadoresArucoMoveis.FindAsync(id);
        if (marcador == null) return NotFound();

        _context.MarcadoresArucoMoveis.Remove(marcador);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}