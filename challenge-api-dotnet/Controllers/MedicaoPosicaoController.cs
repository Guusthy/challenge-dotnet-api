using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/medicoes")]
[Produces("application/json")]
[Tags("Medições de Posição")]
public class MedicaoPosicaoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public MedicaoPosicaoController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar medições de posição")]
    [EndpointDescription("Retorna medições de posição paginadas.")]
    [ProducesResponseType(typeof(PagedResult<Resource<MedicaoPosicaoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MedicaoPosicaoDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.MedicoesPosicoes.AsNoTracking();

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(m => m.IdMedicao) 
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(MedicaoPosicaoMapper.ToDto).ToList();

        
        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMedicao }), "GET"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };

            return new Resource<MedicaoPosicaoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);

        // Links da coleção
        var collectionLinks = Url.PagingLinks(nameof(GetAll), page, size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result = new PagedResult<Resource<MedicaoPosicaoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter medição por ID")]
    [EndpointDescription("Retorna os dados de uma medição de posição pelo seu identificador, com links.")]
    [ProducesResponseType(typeof(Resource<MedicaoPosicaoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MedicaoPosicaoDTO>>> GetById([FromRoute] int id)
    {
        var entidade = await _context.MedicoesPosicoes.FindAsync(id);
        if (entidade == null) return NotFound();

        var dto = MedicaoPosicaoMapper.ToDto(entidade);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("create", Url.ActionHref(nameof(Create)), "POST")
        };

        return Ok(new Resource<MedicaoPosicaoDTO>(dto, links));
    }

    [HttpGet("posicao/{id}")]
    [EndpointSummary("Listar medições por posição")]
    [EndpointDescription("Retorna medições vinculadas a uma posição específica, com paginação.")]
    [ProducesResponseType(typeof(PagedResult<Resource<MedicaoPosicaoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MedicaoPosicaoDTO>>>> GetByPosicaoId(
        [FromRoute] int id,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.MedicoesPosicoes
            .AsNoTracking()
            .Where(m => m.PosicaoIdPosicao == id);

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(m => m.IdMedicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(MedicaoPosicaoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMedicao }), "GET"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST"),
                new("posicao", Url.ActionHref(nameof(GetByPosicaoId), new { id, page = 1, size = 10 }), "GET"),
                new("count-posicao", Url.ActionHref(nameof(CountByPosicaoId), new { id }), "GET")
            };
            return new Resource<MedicaoPosicaoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);

        var collectionLinks = BuildPosicaoPagingLinks(id, page, size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<MedicaoPosicaoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("marcador-fixo/{id}")]
    [EndpointSummary("Listar medições por marcador fixo")]
    [EndpointDescription("Retorna medições associadas a um marcador ArUco fixo, com paginação.")]
    [ProducesResponseType(typeof(PagedResult<Resource<MedicaoPosicaoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MedicaoPosicaoDTO>>>> GetByMarcadorId(
        [FromRoute] int id,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.MedicoesPosicoes
            .AsNoTracking()
            .Where(m => m.MarcadorFixoIdMarcadorArucoFixo == id);

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(m => m.IdMedicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(MedicaoPosicaoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMedicao }), "GET"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST"),
                new("marcador-fixo", Url.ActionHref(nameof(GetByMarcadorId), new { id, page = 1, size = 10 }), "GET")
            };
            return new Resource<MedicaoPosicaoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);

        var collectionLinks = BuildMarcadorPagingLinks(id, page, size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<MedicaoPosicaoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("contagem/posicao/{id}")]
    [EndpointSummary("Contar medições por posição")]
    [EndpointDescription("Retorna a quantidade de medições vinculadas à posição informada.")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> CountByPosicaoId([FromRoute] int id)
    {
        var count = await _context.MedicoesPosicoes
            .AsNoTracking()
            .CountAsync(m => m.PosicaoIdPosicao == id);

        return Ok(count);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar medição de posição")]
    [EndpointDescription("Cria um novo registro de medição de posição.")]
    [ProducesResponseType(typeof(Resource<MedicaoPosicaoDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<MedicaoPosicaoDTO>>> Create([FromBody] MedicaoPosicaoDTO dto)
    {
        var entidade = MedicaoPosicaoMapper.ToEntity(dto);
        _context.MedicoesPosicoes.Add(entidade);
        await _context.SaveChangesAsync();

        var resp = MedicaoPosicaoMapper.ToDto(entidade);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = resp.IdMedicao }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        return CreatedAtAction(nameof(GetById),
            new { id = resp.IdMedicao },
            new Resource<MedicaoPosicaoDTO>(resp, links));
    }

    // Método auxiliar para rotas com parâmetro no caminho
    private IEnumerable<HateoasLink> BuildPosicaoPagingLinks(int posicaoId, int page, int size, int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByPosicaoId), new { id = posicaoId, page, size }), "GET"),
            new("first", Url.ActionHref(nameof(GetByPosicaoId), new { id = posicaoId, page = 1, size }), "GET"),
            new("last",
                Url.ActionHref(nameof(GetByPosicaoId),
                    new { id = posicaoId, page = totalPages > 0 ? totalPages : 1, size }), "GET")
        };

        if (page > 1)
            links.Add(new("prev", Url.ActionHref(nameof(GetByPosicaoId), new { id = posicaoId, page = page - 1, size }),
                "GET"));

        if (totalPages > 0 && page < totalPages)
            links.Add(new("next", Url.ActionHref(nameof(GetByPosicaoId), new { id = posicaoId, page = page + 1, size }),
                "GET"));

        return links;
    }

    private IEnumerable<HateoasLink> BuildMarcadorPagingLinks(int marcadorFixoId, int page, int size, int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByMarcadorId), new { id = marcadorFixoId, page, size }), "GET"),
            new("first", Url.ActionHref(nameof(GetByMarcadorId), new { id = marcadorFixoId, page = 1, size }), "GET"),
            new("last",
                Url.ActionHref(nameof(GetByMarcadorId),
                    new { id = marcadorFixoId, page = totalPages > 0 ? totalPages : 1, size }), "GET")
        };

        if (page > 1)
            links.Add(new("prev",
                Url.ActionHref(nameof(GetByMarcadorId), new { id = marcadorFixoId, page = page - 1, size }), "GET"));

        if (totalPages > 0 && page < totalPages)
            links.Add(new("next",
                Url.ActionHref(nameof(GetByMarcadorId), new { id = marcadorFixoId, page = page + 1, size }), "GET"));

        return links;
    }
}