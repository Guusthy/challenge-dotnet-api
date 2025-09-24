using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/marcadores-fixos")]
[Produces("application/json")]
[Tags("Marcadores ArUco Fixos")]
public class MarcadorFixoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public MarcadorFixoController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar marcadores fixos")]
    [EndpointDescription("Retorna marcadores ArUco fixos paginados.")]
    [ProducesResponseType(typeof(PagedResult<Resource<MarcadorFixoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MarcadorFixoDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.MarcadoresFixos.AsNoTracking();

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(m => m.IdMarcadorArucoFixo)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(MarcadorFixoMapper.ToDto).ToList();

        // HATEOAS por item
        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMarcadorArucoFixo }), "GET"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdMarcadorArucoFixo }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };

            return new Resource<MarcadorFixoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);

        // Links da coleção
        var collectionLinks = Url.PagingLinks(nameof(GetAll), page, size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result = new PagedResult<Resource<MarcadorFixoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter marcador fixo por ID")]
    [EndpointDescription("Retorna um marcador ArUco fixo com links de navegação e ações.")]
    [ProducesResponseType(typeof(Resource<MarcadorFixoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MarcadorFixoDTO>>> GetById([FromRoute] int id)
    {
        var entidade = await _context.MarcadoresFixos.FindAsync(id);
        if (entidade == null) return NotFound();

        var dto = MarcadorFixoMapper.ToDto(entidade);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("create", Url.ActionHref(nameof(Create)), "POST")
        };

        return Ok(new Resource<MarcadorFixoDTO>(dto, links));
    }

    [HttpGet("patio/{patioId}")]
    [EndpointSummary("Listar marcadores fixos por pátio")]
    [EndpointDescription("Retorna marcadores fixos associados ao pátio informado, com paginação.")]
    [ProducesResponseType(typeof(PagedResult<Resource<MarcadorFixoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MarcadorFixoDTO>>>> GetByPatioId(
        [FromRoute] int patioId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.MarcadoresFixos
            .AsNoTracking()
            .Where(m => m.PatioIdPatio == patioId);

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(m => m.IdMarcadorArucoFixo)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(MarcadorFixoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMarcadorArucoFixo }), "GET"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdMarcadorArucoFixo }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };
            return new Resource<MarcadorFixoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);

        //Links da coleção com rota que inclui patioId
        var collectionLinks = BuildPatioPagingLinks(patioId, page, size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<MarcadorFixoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("busca")]
    [EndpointSummary("Buscar marcador fixo por código ArUco")]
    [EndpointDescription("Retorna um marcador fixo filtrado pelo código (Ex: ARUCO_001)")]
    [ProducesResponseType(typeof(Resource<MarcadorFixoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MarcadorFixoDTO>>> GetByCodigoAruco([FromQuery] string codigoAruco)
    {
        var entidade = await _context.MarcadoresFixos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CodigoAruco.ToLower() == codigoAruco.ToLower());

        if (entidade == null) return NotFound();

        var dto = MarcadorFixoMapper.ToDto(entidade);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByCodigoAruco), new { codigoAruco }), "GET"),
            new("marcador-by-id", Url.ActionHref(nameof(GetById), new { id = dto.IdMarcadorArucoFixo }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("create", Url.ActionHref(nameof(Create)), "POST")
        };

        return Ok(new Resource<MarcadorFixoDTO>(dto, links));
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar marcador fixo")]
    [EndpointDescription("Cria um novo marcador ArUco fixo.")]
    [ProducesResponseType(typeof(Resource<MarcadorFixoDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<MarcadorFixoDTO>>> Create([FromBody] MarcadorFixoDTO dto)
    {
        var entidade = MarcadorFixoMapper.ToEntity(dto);
        _context.MarcadoresFixos.Add(entidade);
        await _context.SaveChangesAsync();

        var resp = MarcadorFixoMapper.ToDto(entidade);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = resp.IdMarcadorArucoFixo }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id = resp.IdMarcadorArucoFixo }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        return CreatedAtAction(nameof(GetById),
            new { id = resp.IdMarcadorArucoFixo },
            new Resource<MarcadorFixoDTO>(resp, links));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir marcador fixo")]
    [EndpointDescription("Remove um marcador ArUco fixo do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var entidade = await _context.MarcadoresFixos.FindAsync(id);
        if (entidade == null) return NotFound();

        _context.MarcadoresFixos.Remove(entidade);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Método para auxiliar a paginação do endpoint com rota que inclui patioId
    private IEnumerable<HateoasLink> BuildPatioPagingLinks(int patioId, int page, int size, int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByPatioId), new { patioId, page, size }), "GET"),
            new("first", Url.ActionHref(nameof(GetByPatioId), new { patioId, page = 1, size }), "GET"),
            new("last",
                Url.ActionHref(nameof(GetByPatioId), new { patioId, page = totalPages > 0 ? totalPages : 1, size }),
                "GET")
        };

        if (page > 1)
            links.Add(new("prev", Url.ActionHref(nameof(GetByPatioId), new { patioId, page = page - 1, size }), "GET"));

        if (totalPages > 0 && page < totalPages)
            links.Add(new("next", Url.ActionHref(nameof(GetByPatioId), new { patioId, page = page + 1, size }), "GET"));

        return links;
    }
}