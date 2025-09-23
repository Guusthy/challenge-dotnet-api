using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/patio")]
[Produces("application/json")]
[Tags("Pátios")]
public class PatioController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PatioController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [EndpointSummary("Listar pátios")]
    [EndpointDescription("Retorna todos os pátios cadastrados.")]
    [ProducesResponseType(typeof(List<PatioDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PatioDTO>>> GetAll()
    {
        var patios = await _context.Patios.ToListAsync();
        return patios.Select(PatioMapper.ToDto).ToList();
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter pátio por ID")]
    [EndpointDescription("Retorna os dados do pátio especificado pelo identificador.")]
    [ProducesResponseType(typeof(PatioDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatioDTO>> GetById([FromRoute] int id)
    {
        var patio = await _context.Patios.FindAsync(id);
        if (patio == null)
        {
            return NotFound();
        }
        return PatioMapper.ToDto(patio);
    }
    
    [HttpGet("com-motos")]
    [EndpointSummary("Listar pátios com dados vinculados")]
    [EndpointDescription("Retorna pátios que possuem usuários, posições ou marcadores fixos vinculados.")]
    [ProducesResponseType(typeof(List<PatioDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PatioDTO>>> GetPatiosComMotos()
    {
        var patios = await _context.Patios
            .Where(p => p.Usuarios.Any() || p.Posicoes.Any() || p.MarcadoresFixos.Any())
            .ToListAsync();

        return patios.Select(PatioMapper.ToDto).ToList();
    }

    [HttpGet("{id}/motos")]
    [EndpointSummary("Listar motos de um pátio")]
    [EndpointDescription("Retorna todas as motos atualmente associadas ao pátio informado.")]
    [ProducesResponseType(typeof(List<MotoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MotoDTO>>> GetMotosPorPatio([FromRoute] int id)
    {
        var motos = await _context.Posicoes
            .Where(p => p.PatioIdPatio == id && p.MotoIdMoto != null)
            .Include(p => p.MotoIdMotoNavigation)
            .Select(p => p.MotoIdMotoNavigation)
            .Distinct()
            .ToListAsync();
        return motos.Select(MotoMapper.ToDto).ToList();
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar pátio")]
    [EndpointDescription("Cria um novo pátio.")]
    [ProducesResponseType(typeof(PatioDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatioDTO>> Create([FromBody] PatioDTO dto)
    {
        var patio = PatioMapper.ToEntity(dto);
        _context.Patios.Add(patio);
        await _context.SaveChangesAsync();
        
        var response = PatioMapper.ToDto(patio);
        return CreatedAtAction(nameof(GetById), new { id = patio.IdPatio }, response);
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar pátio")]
    [EndpointDescription("Atualiza os dados de um pátio existente.")]
    [ProducesResponseType(typeof(PatioDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatioDTO>> Update([FromRoute] int id,[FromBody]  PatioDTO dto)
    {
        if (id != dto.IdPatio)
        {
            return BadRequest();
        }
        var patio = await _context.Patios.FindAsync(id);
        if (patio == null)
        {
            return NotFound();
        }

        patio.Nome = dto.Nome;
        patio.Localizacao = dto.Localizacao;
        patio.Descricao = dto.Descricao;

        await _context.SaveChangesAsync();
        return Ok(PatioMapper.ToDto(patio));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir pátio")]
    [EndpointDescription("Remove um pátio do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatioDTO>> Delete([FromRoute] int id)
    {
        var patio = await _context.Patios.FindAsync(id);
        if (patio == null)
        {
            return NotFound();
        }
        _context.Patios.Remove(patio);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}