using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/moto")]
[Produces("application/json")]
[Tags("Motos")]
public class MotoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public MotoController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [EndpointSummary("Listar motos")]
    [EndpointDescription("Retorna todas as motos cadastradas.")]
    [ProducesResponseType(typeof(List<MotoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MotoDTO>>> GetAll()
    {
        var motos = await _context.Motos.ToListAsync();
        return motos.Select(MotoMapper.ToDto).ToList();
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter moto por ID")]
    [EndpointDescription("Retorna os dados da moto especificada pelo identificador.")]
    [ProducesResponseType(typeof(MotoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MotoDTO>> GetById([FromRoute] int id)
    {
        var moto = await _context.Motos.FindAsync(id);
        if (moto == null)
        {
            return NotFound();
        }
        return MotoMapper.ToDto(moto);
    }

    [HttpGet("placa/{placa}")]
    [EndpointSummary("Buscar por placa (prefixo)")]
    [EndpointDescription("Retorna motos cuja placa começa com o prefixo informado (ex.: THJ4Y67).")]
    [ProducesResponseType(typeof(List<MotoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MotoDTO>>> GetByPlaca([FromRoute] string placa)
    {
        var motos = await _context.Motos
            .Where(m => m.Placa.StartsWith(placa))
            .ToListAsync();
        
        if (!motos.Any())
        {
            return NotFound();
        }   
        return motos.Select(MotoMapper.ToDto).ToList();
    }

    [HttpGet("status/{status}")]
    [EndpointSummary("Listar por status")]
    [EndpointDescription("Retorna motos filtradas pelo status (ex.: Pronta, Sem peça, Motor).")]
    [ProducesResponseType(typeof(List<MotoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MotoDTO>>> GetByStatus([FromRoute] string status)
    {
        var motos = await _context.Motos
            .Where(m => m.Status.ToLower() == status.ToLower())
            .ToListAsync();
        return motos.Select(MotoMapper.ToDto).ToList();
    }

    [HttpGet("{id}/posicoes")]
    [EndpointSummary("Listar posições da moto")]
    [EndpointDescription("Retorna as posições registradas para a moto informada.")]
    [ProducesResponseType(typeof(List<PosicaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PosicaoDTO>>> GetByPosicoesMoto([FromRoute] int id)
    {
        var posicoes = await  _context.Posicoes
            .Where(p => p.MotoIdMoto == id)
            .ToListAsync();
        return posicoes.Select(PosicaoMapper.ToDto).ToList();
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar moto")]
    [EndpointDescription("Cria uma nova moto.")]
    [ProducesResponseType(typeof(MotoDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MotoCreateDTO>> Create([FromBody] MotoCreateDTO dto)
    {
        var moto = MotoMapper.ToEntity(dto);
        moto.DataCadastro = DateTime.Now;
        _context.Add(moto);
        await _context.SaveChangesAsync();

        var response = MotoMapper.ToDto(moto);
        return CreatedAtAction(nameof(GetById), new { id = moto.IdMoto}, response);
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar moto")]
    [EndpointDescription("Atualiza os dados de uma moto existente.")]
    [ProducesResponseType(typeof(MotoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MotoCreateDTO>> Update([FromRoute] int id,[FromBody] MotoCreateDTO dto)
    {
        if (id != dto.IdMoto)
        {
            return BadRequest();
        }
        var moto = await _context.Motos.FindAsync(id);
        if (moto == null)
        {
            return NotFound();
        }
        
        moto.Placa = dto.Placa;
        moto.Modelo = dto.Modelo;
        moto.Status = dto.Status;
        
        await _context.SaveChangesAsync();
        return Ok(MotoMapper.ToDto(moto));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir moto")]
    [EndpointDescription("Remove uma moto do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MotoDTO>> Delete([FromRoute] int id)
    {
        var moto = await _context.Motos.FindAsync(id);
        if (moto == null)
        {
            return NotFound();
        }
        _context.Motos.Remove(moto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}