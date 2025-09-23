using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/marcador-fixo")]
[Produces("application/json")]
[Tags("Marcadores ArUco Fixos")]
public class MarcadorFixoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public MarcadorFixoController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar marcadores fixos")]
    [EndpointDescription("Retorna todos os marcadores ArUco fixos cadastrados.")]
    [ProducesResponseType(typeof(List<MarcadorFixoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MarcadorFixoDTO>>> GetAll()
    {
        var marcadores = await _context.MarcadoresFixos.ToListAsync();
        return marcadores.Select(MarcadorFixoMapper.ToDto).ToList();
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter marcador fixo por ID")]
    [EndpointDescription("Retorna os dados de um marcador ArUco fixo a partir do seu identificador.")]
    [ProducesResponseType(typeof(MarcadorFixoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MarcadorFixoDTO>> GetById([FromRoute] int id)
    {
        var marcador = await _context.MarcadoresFixos.FindAsync(id);
        if (marcador == null) return NotFound();
        return MarcadorFixoMapper.ToDto(marcador);
    }

    [HttpGet("patio/{patioId}")]
    [EndpointSummary("Listar marcadores fixos por pátio")]
    [EndpointDescription("Retorna todos os marcadores fixos associados ao pátio informado.")]
    [ProducesResponseType(typeof(List<MarcadorFixoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MarcadorFixoDTO>>> GetByPatioId([FromRoute] int patioId)
    {
        var marcadores = await _context.MarcadoresFixos
            .Where(m => m.PatioIdPatio == patioId)
            .ToListAsync();

        return marcadores.Select(MarcadorFixoMapper.ToDto).ToList();
    }

    [HttpGet("busca")]
    [EndpointSummary("Buscar marcador fixo por código ArUco")]
    [EndpointDescription("Retorna um marcador ArUco fixo filtrado pelo código passado na querystring. Ex: ARUCO_003")]
    [ProducesResponseType(typeof(MarcadorFixoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MarcadorFixoDTO>> GetByCodigoAruco([FromQuery] string codigoAruco)
    {
        var marcador = await _context.MarcadoresFixos
            .FirstOrDefaultAsync(m => m.CodigoAruco.ToLower() == codigoAruco.ToLower());

        if (marcador == null) return NotFound();
        return MarcadorFixoMapper.ToDto(marcador);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar marcador fixo")]
    [EndpointDescription("Cria um novo marcador ArUco fixo.")]
    [ProducesResponseType(typeof(MarcadorFixoDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MarcadorFixoDTO>> Create([FromBody] MarcadorFixoDTO dto)
    {
        var marcador = MarcadorFixoMapper.ToEntity(dto);
        _context.MarcadoresFixos.Add(marcador);
        await _context.SaveChangesAsync();

        var response = MarcadorFixoMapper.ToDto(marcador);
        return CreatedAtAction(nameof(GetById), new { id = marcador.IdMarcadorArucoFixo }, response);
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir marcador fixo")]
    [EndpointDescription("Remove um marcador ArUco fixo do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var marcador = await _context.MarcadoresFixos.FindAsync(id);
        if (marcador == null) return NotFound();

        _context.MarcadoresFixos.Remove(marcador);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}