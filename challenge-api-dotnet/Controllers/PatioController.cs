using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/patios")]
[Produces("application/json")]
[Tags("Pátios")]
public class PatioController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public PatioController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar pátios")]
    [EndpointDescription("Retorna pátios paginados.")]
    [ProducesResponseType(typeof(PagedResult<Resource<PatioDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<PatioDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.Patios.AsNoTracking();

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(p => p.IdPatio)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(PatioMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdPatio }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdPatio }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdPatio }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST"),
                new("motos", Url.ActionHref(nameof(GetMotosPorPatio), new { id = dto.IdPatio }), "GET")
            };
            return new Resource<PatioDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);
        var collectionLinks = Url.PagingLinks(nameof(GetAll), page, size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result = new PagedResult<Resource<PatioDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter pátio por ID")]
    [EndpointDescription("Retorna os dados do pátio com links de navegação e ações.")]
    [ProducesResponseType(typeof(Resource<PatioDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<PatioDTO>>> GetById([FromRoute] int id)
    {
        var entidade = await _context.Patios.FindAsync(id);
        if (entidade == null) return NotFound();

        var dto = PatioMapper.ToDto(entidade);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("create", Url.ActionHref(nameof(Create)), "POST"),
            new("motos", Url.ActionHref(nameof(GetMotosPorPatio), new { id }), "GET")
        };

        return Ok(new Resource<PatioDTO>(dto, links));
    }

    [HttpGet("com-motos")]
    [EndpointSummary("Listar pátios com dados vinculados")]
    [EndpointDescription("Retorna pátios que possuem usuários, posições ou marcadores fixos vinculados.")]
    [ProducesResponseType(typeof(PagedResult<Resource<PatioDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<PatioDTO>>>> GetPatiosComMotos(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.Patios
            .AsNoTracking()
            .Where(p => p.Usuarios.Any() || p.Posicoes.Any() || p.MarcadoresFixos.Any());

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(p => p.IdPatio)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(PatioMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdPatio }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdPatio }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdPatio }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST"),
                new("motos", Url.ActionHref(nameof(GetMotosPorPatio), new { id = dto.IdPatio }), "GET")
            };
            return new Resource<PatioDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);
        var collectionLinks = Url.PagingLinks(nameof(GetPatiosComMotos), page, size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<PatioDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}/motos")]
    [EndpointSummary("Listar motos de um pátio")]
    [EndpointDescription("Retorna todas as motos atualmente associadas ao pátio informado.")]
    [ProducesResponseType(typeof(List<MotoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MotoDTO>>> GetMotosPorPatio([FromRoute] int id)
    {
        var motos = await _context.Posicoes
            .AsNoTracking()
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
    [ProducesResponseType(typeof(Resource<PatioDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<PatioDTO>>> Create([FromBody] PatioDTO dto)
    {
        var entidade = PatioMapper.ToEntity(dto);
        _context.Patios.Add(entidade);
        await _context.SaveChangesAsync();

        var response = PatioMapper.ToDto(entidade);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = entidade.IdPatio }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id = entidade.IdPatio }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id = entidade.IdPatio }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("motos", Url.ActionHref(nameof(GetMotosPorPatio), new { id = entidade.IdPatio }), "GET")
        };

        return CreatedAtAction(nameof(GetById),
            new { id = entidade.IdPatio },
            new Resource<PatioDTO>(response, links));
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar pátio")]
    [EndpointDescription("Atualiza os dados de um pátio existente.")]
    [ProducesResponseType(typeof(Resource<PatioDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<PatioDTO>>> Update([FromRoute] int id, [FromBody] PatioDTO dto)
    {
        if (id != dto.IdPatio) return BadRequest();

        var entidade = await _context.Patios.FindAsync(id);
        if (entidade == null) return NotFound();

        entidade.Nome = dto.Nome;
        entidade.Localizacao = dto.Localizacao;
        entidade.Descricao = dto.Descricao;

        await _context.SaveChangesAsync();

        var response = PatioMapper.ToDto(entidade);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("motos", Url.ActionHref(nameof(GetMotosPorPatio), new { id }), "GET")
        };

        return Ok(new Resource<PatioDTO>(response, links));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir pátio")]
    [EndpointDescription("Remove um pátio do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var entidade = await _context.Patios.FindAsync(id);
        if (entidade == null) return NotFound();

        _context.Patios.Remove(entidade);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}