using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/motos")]
[Produces("application/json")]
[Tags("Motos")]
public class MotoController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MotoController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar motos")]
    [EndpointDescription("Retorna motos paginadas.")]
    [ProducesResponseType(typeof(PagedResult<Resource<MotoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MotoDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.Motos.AsNoTracking();

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(m => m.IdMoto)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(MotoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMoto }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdMoto }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdMoto }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };
            return new Resource<MotoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);
        var collectionLinks = Url.PagingLinks(nameof(GetAll), page, size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result = new PagedResult<Resource<MotoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }


    [HttpGet("{id}")]
    [EndpointSummary("Obter moto por ID")]
    [EndpointDescription("Retorna os dados da moto especificada pelo identificador, com links.")]
    [ProducesResponseType(typeof(Resource<MotoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MotoDTO>>> GetById([FromRoute] int id)
    {
        var moto = await _context.Motos.FindAsync(id);
        if (moto == null) return NotFound();

        var dto = MotoMapper.ToDto(moto);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("create", Url.ActionHref(nameof(Create)), "POST")
        };

        return Ok(new Resource<MotoDTO>(dto, links));
    }

    [HttpGet("placa/{placa}")]
    [EndpointSummary("Buscar por placa (prefixo)")]
    [EndpointDescription("Retorna motos cuja placa começa com o prefixo informado (Ex: ABC1D23 )")]
    [ProducesResponseType(typeof(PagedResult<Resource<MotoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MotoDTO>>>> GetByPlaca(
        [FromRoute] string placa,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.Motos
            .AsNoTracking()
            .Where(m => m.Placa.StartsWith(placa));

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(m => m.IdMoto)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(MotoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMoto }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdMoto }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdMoto }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };
            return new Resource<MotoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);
        var collectionLinks = BuildPlacaPagingLinks(placa, page, size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<MotoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    [EndpointSummary("Listar por status")]
    [EndpointDescription("Retorna motos filtradas pelo status (ex.: Pronta, Sem peça, Motor).")]
    [ProducesResponseType(typeof(PagedResult<Resource<MotoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MotoDTO>>>> GetByStatus(
        [FromRoute] string status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.Motos
            .AsNoTracking()
            .Where(m => m.Status.ToLower() == status.ToLower());

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(m => m.IdMoto)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(MotoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMoto }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdMoto }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdMoto }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };
            return new Resource<MotoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);
        var collectionLinks = BuildStatusPagingLinks(status, page, size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<MotoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}/posicoes")]
    [EndpointSummary("Listar posições da moto")]
    [EndpointDescription("Retorna as posições registradas para a moto informada.")]
    [ProducesResponseType(typeof(List<PosicaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PosicaoDTO>>> GetByPosicoesMoto([FromRoute] int id)
    {
        var posicoes = await _context.Posicoes
            .AsNoTracking()
            .Where(p => p.MotoIdMoto == id)
            .ToListAsync();

        return posicoes.Select(PosicaoMapper.ToDto).ToList();
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar moto")]
    [EndpointDescription("Cria uma nova moto.")]
    [ProducesResponseType(typeof(Resource<MotoDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<MotoDTO>>> Create([FromBody] MotoCreateDTO dto)
    {
        var moto = MotoMapper.ToEntity(dto);
        moto.DataCadastro = DateTime.Now;
        _context.Add(moto);
        await _context.SaveChangesAsync();

        var response = MotoMapper.ToDto(moto);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = moto.IdMoto }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id = moto.IdMoto }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id = moto.IdMoto }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        return CreatedAtAction(nameof(GetById),
            new { id = moto.IdMoto },
            new Resource<MotoDTO>(response, links));
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar moto")]
    [EndpointDescription("Atualiza os dados de uma moto existente.")]
    [ProducesResponseType(typeof(Resource<MotoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MotoDTO>>> Update([FromRoute] int id, [FromBody] MotoCreateDTO dto)
    {
        if (id != dto.IdMoto) return BadRequest();

        var moto = await _context.Motos.FindAsync(id);
        if (moto == null) return NotFound();

        moto.Placa = dto.Placa;
        moto.Modelo = dto.Modelo;
        moto.Status = dto.Status;

        await _context.SaveChangesAsync();

        var response = MotoMapper.ToDto(moto);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        return Ok(new Resource<MotoDTO>(response, links));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir moto")]
    [EndpointDescription("Remove uma moto do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var moto = await _context.Motos.FindAsync(id);
        if (moto == null) return NotFound();

        _context.Motos.Remove(moto);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Métodos auxiliares para rotas com parâmetro no caminho
    private IEnumerable<HateoasLink> BuildPlacaPagingLinks(string placa, int page, int size, int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByPlaca), new { placa, page, size }), "GET"),
            new("first", Url.ActionHref(nameof(GetByPlaca), new { placa, page = 1, size }), "GET"),
            new("last", Url.ActionHref(nameof(GetByPlaca), new { placa, page = totalPages > 0 ? totalPages : 1, size }),
                "GET")
        };
        if (page > 1)
            links.Add(new("prev", Url.ActionHref(nameof(GetByPlaca), new { placa, page = page - 1, size }), "GET"));
        if (totalPages > 0 && page < totalPages)
            links.Add(new("next", Url.ActionHref(nameof(GetByPlaca), new { placa, page = page + 1, size }), "GET"));
        return links;
    }

    private IEnumerable<HateoasLink> BuildStatusPagingLinks(string status, int page, int size, int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByStatus), new { status, page, size }), "GET"),
            new("first", Url.ActionHref(nameof(GetByStatus), new { status, page = 1, size }), "GET"),
            new("last",
                Url.ActionHref(nameof(GetByStatus), new { status, page = totalPages > 0 ? totalPages : 1, size }),
                "GET")
        };
        if (page > 1)
            links.Add(new("prev", Url.ActionHref(nameof(GetByStatus), new { status, page = page - 1, size }), "GET"));
        if (totalPages > 0 && page < totalPages)
            links.Add(new("next", Url.ActionHref(nameof(GetByStatus), new { status, page = page + 1, size }), "GET"));
        return links;
    }
}