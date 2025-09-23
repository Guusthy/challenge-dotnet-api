using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/posicao")]
[Produces("application/json")]
[Tags("Posições")]
public class PosicaoController :  ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PosicaoController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [EndpointSummary("Listar posições")]
    [EndpointDescription("Retorna todas as posições registradas no sistema.")]
    [ProducesResponseType(typeof(List<PosicaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PosicaoDTO>>> GetAll()
    {
        var posicoes = await _context.Posicoes.ToListAsync();
        return posicoes.Select(PosicaoMapper.ToDto).ToList();
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter posição por ID")]
    [EndpointDescription("Retorna os dados da posição especificada pelo identificador.")]
    [ProducesResponseType(typeof(PosicaoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PosicaoDTO>> GetById([FromRoute] int id)
    {
        var posicao = await _context.Posicoes.FindAsync(id);
        if (posicao == null)
        {
            return NotFound();
        }
        return PosicaoMapper.ToDto(posicao);
    }
    
    [HttpGet("moto/{motoId}")]
    [EndpointSummary("Listar posições de uma moto")]
    [EndpointDescription("Retorna todas as posições associadas a uma moto específica.")]
    [ProducesResponseType(typeof(List<PosicaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PosicaoDTO>>> GetByMotoId([FromRoute] int motoId)
    {
        var posicoes = await _context.Posicoes
            .Where(p => p.MotoIdMoto == motoId)
            .ToListAsync();

        return posicoes.Select(PosicaoMapper.ToDto).ToList();
    }
    
    [HttpGet("historico/{motoId}")]
    [EndpointSummary("Histórico de posições da moto")]
    [EndpointDescription("Retorna o histórico de posições de uma moto, ordenado por data decrescente.")]
    [ProducesResponseType(typeof(List<PosicaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PosicaoDTO>>> GetHistoricoDaMoto([FromRoute] int motoId)
    {
        var posicoes = await _context.Posicoes
            .Where(p => p.MotoIdMoto == motoId)
            .OrderByDescending(p => p.DataHora)
            .ToListAsync();

        return posicoes.Select(PosicaoMapper.ToDto).ToList();
    }
    
    [HttpGet("motos-revisao")]
    [EndpointSummary("Listar posições de motos em revisão")]
    [EndpointDescription("Retorna as posições atuais de todas as motos com status 'Revisão'.")]
    [ProducesResponseType(typeof(List<PosicaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PosicaoDTO>>> GetPosicoesDeMotosRevisao()
    {
        var posicoes = await _context.Posicoes
            .Include(p => p.MotoIdMotoNavigation)
            .Where(p => p.MotoIdMotoNavigation != null && p.MotoIdMotoNavigation.Status.ToLower() == "revisão")
            .ToListAsync();

        return posicoes.Select(PosicaoMapper.ToDto).ToList();
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar posição")]
    [EndpointDescription("Cria uma nova posição.")]
    [ProducesResponseType(typeof(PosicaoDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    public async Task<ActionResult<PosicaoDTO>> Create([FromBody] PosicaoDTO dto)
    {
        var posicao = PosicaoMapper.ToEntity(dto);
        _context.Posicoes.Add(posicao);
        await _context.SaveChangesAsync();
        
        var response = PosicaoMapper.ToDto(posicao);
        return CreatedAtAction(nameof(GetById), new { id = posicao.IdPosicao }, response);
    }
    
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar posição")]
    [EndpointDescription("Atualiza os dados de uma posição existente.")]
    [ProducesResponseType(typeof(PosicaoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PosicaoDTO>> Update([FromRoute] int id,[FromBody] PosicaoDTO dto)
    {
        if (id != dto.IdPosicao)
        {
            return BadRequest();
        }

        var posicao = await _context.Posicoes.FindAsync(id);
        if (posicao == null)
        {
            return NotFound();
        }

        posicao.XPos = dto.XPos;
        posicao.YPos = dto.YPos;
        posicao.DataHora = dto.DataHora;
        posicao.MotoIdMoto = dto.MotoId;
        posicao.PatioIdPatio = dto.PatioId;

        await _context.SaveChangesAsync();
        return Ok(PosicaoMapper.ToDto(posicao));
    }
    
    [HttpDelete("{id}")]
    [EndpointSummary("Excluir posição")]
    [EndpointDescription("Remove uma posição do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PosicaoDTO>> Delete([FromRoute] int id)
    {
        var posicao = await _context.Posicoes.FindAsync(id);
        if (posicao == null)
        {
            return NotFound();
        }

        _context.Posicoes.Remove(posicao);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}