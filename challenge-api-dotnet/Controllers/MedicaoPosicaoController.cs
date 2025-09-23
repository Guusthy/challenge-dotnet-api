using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/medicao-posicao")]
[Produces("application/json")]
[Tags("Medições de Posição")]
public class MedicaoPosicaoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public MedicaoPosicaoController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar medições de posição")]
    [EndpointDescription("Retorna todas as medições de posição registradas.")]
    [ProducesResponseType(typeof(List<MedicaoPosicaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MedicaoPosicaoDTO>>> GetAll()
    {
        var medicoes = await _context.MedicoesPosicoes.ToListAsync();
        return medicoes.Select(MedicaoPosicaoMapper.ToDto).ToList();
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter medição por ID")]
    [EndpointDescription("Retorna os dados de uma medição de posição pelo seu identificador.")]
    [ProducesResponseType(typeof(MedicaoPosicaoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicaoPosicaoDTO>> GetById([FromRoute] int id)
    {
        var medicao = await _context.MedicoesPosicoes.FindAsync(id);
        if (medicao == null) return NotFound();
        return MedicaoPosicaoMapper.ToDto(medicao);
    }

    [HttpGet("posicao/{id}")]
    [EndpointSummary("Listar medições por posição")]
    [EndpointDescription("Retorna todas as medições vinculadas a uma posição específica.")]
    [ProducesResponseType(typeof(List<MedicaoPosicaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MedicaoPosicaoDTO>>> GetByPosicaoId([FromRoute] int id)
    {
        var medicoes = await _context.MedicoesPosicoes
            .Where(m => m.PosicaoIdPosicao == id)
            .ToListAsync();

        return medicoes.Select(MedicaoPosicaoMapper.ToDto).ToList();
    }

    [HttpGet("marcador-fixo/{id}")]
    [EndpointSummary("Listar medições por marcador fixo")]
    [EndpointDescription("Retorna todas as medições associadas a um marcador ArUco fixo.")]
    [ProducesResponseType(typeof(List<MedicaoPosicaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MedicaoPosicaoDTO>>> GetByMarcadorId([FromRoute] int id)
    {
        var medicoes = await _context.MedicoesPosicoes
            .Where(m => m.MarcadorFixoIdMarcadorArucoFixo == id)
            .ToListAsync();

        return medicoes.Select(MedicaoPosicaoMapper.ToDto).ToList();
    }

    [HttpGet("contagem/posicao/{id}")]
    [EndpointSummary("Contar medições por posição")]
    [EndpointDescription("Retorna a quantidade de medições vinculadas à posição informada.")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> CountByPosicaoId([FromRoute] int id)
    {
        var count = await _context.MedicoesPosicoes
            .CountAsync(m => m.PosicaoIdPosicao == id);

        return Ok(count);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar medição de posição")]
    [EndpointDescription("Cria um novo registro de medição de posição.")]
    [ProducesResponseType(typeof(MedicaoPosicaoDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MedicaoPosicaoDTO>> Create([FromBody] MedicaoPosicaoDTO dto)
    {
        var medicao = MedicaoPosicaoMapper.ToEntity(dto);
        _context.MedicoesPosicoes.Add(medicao);
        await _context.SaveChangesAsync();

        var response = MedicaoPosicaoMapper.ToDto(medicao);
        return CreatedAtAction(nameof(GetById), new { id = medicao.IdMedicao }, response);
    }
}