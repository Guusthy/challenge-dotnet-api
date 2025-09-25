using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/posicao")]
[Produces("application/json")]
[Tags("Posições")]
public class PosicaoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public PosicaoController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar posições")]
    [EndpointDescription("Retorna posições paginadas.")]
    [ProducesResponseType(typeof(PagedResult<Resource<PosicaoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<PosicaoDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.Posicoes.AsNoTracking();

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(p => p.IdPosicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(PosicaoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdPosicao }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdPosicao }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdPosicao }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };

            if (dto.MotoId is not null)
            {
                links.Add(new("moto-posicoes",
                    Url.ActionHref(nameof(GetByMotoId), new { motoId = dto.MotoId, page = 1, size = 10 }), "GET"));
                links.Add(new("historico",
                    Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId = dto.MotoId, page = 1, size = 10 }),
                    "GET"));
            }

            return new Resource<PosicaoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);
        var collectionLinks = Url.PagingLinks(nameof(GetAll), page, size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result = new PagedResult<Resource<PosicaoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter posição por ID")]
    [EndpointDescription("Retorna os dados da posição especificada, com links.")]
    [ProducesResponseType(typeof(Resource<PosicaoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<PosicaoDTO>>> GetById([FromRoute] int id)
    {
        var ent = await _context.Posicoes.FindAsync(id);
        if (ent == null) return NotFound();

        var dto = PosicaoMapper.ToDto(ent);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("create", Url.ActionHref(nameof(Create)), "POST")
        };

        if (dto.MotoId is not null)
        {
            links.Add(new("moto-posicoes",
                Url.ActionHref(nameof(GetByMotoId), new { motoId = dto.MotoId, page = 1, size = 10 }), "GET"));
            links.Add(new("historico",
                Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId = dto.MotoId, page = 1, size = 10 }), "GET"));
        }

        return Ok(new Resource<PosicaoDTO>(dto, links));
    }

    [HttpGet("moto/{motoId}")]
    [EndpointSummary("Listar posições de uma moto")]
    [EndpointDescription("Retorna posições associadas a uma moto específica.")]
    [ProducesResponseType(typeof(PagedResult<Resource<PosicaoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<PosicaoDTO>>>> GetByMotoId(
        [FromRoute] int motoId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.Posicoes
            .AsNoTracking()
            .Where(p => p.MotoIdMoto == motoId);

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(p => p.IdPosicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(PosicaoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdPosicao }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdPosicao }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdPosicao }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST"),
                new("moto-posicoes", Url.ActionHref(nameof(GetByMotoId), new { motoId, page = 1, size = 10 }), "GET"),
                new("historico", Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId, page = 1, size = 10 }), "GET")
            };
            return new Resource<PosicaoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);
        var collectionLinks = BuildMotoPagingLinks(motoId, page, size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<PosicaoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("historico/{motoId}")]
    [EndpointSummary("Histórico de posições da moto ")]
    [EndpointDescription("Retorna o histórico de posições de uma moto, ordenado por data decrescente.")]
    [ProducesResponseType(typeof(PagedResult<Resource<PosicaoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<PosicaoDTO>>>> GetHistoricoDaMoto(
        [FromRoute] int motoId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.Posicoes
            .AsNoTracking()
            .Where(p => p.MotoIdMoto == motoId);

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderByDescending(p => p.DataHora)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(PosicaoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdPosicao }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdPosicao }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdPosicao }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST"),
                new("moto-posicoes", Url.ActionHref(nameof(GetByMotoId), new { motoId, page = 1, size = 10 }), "GET"),
                new("historico", Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId, page = 1, size = 10 }), "GET")
            };
            return new Resource<PosicaoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);
        var collectionLinks = BuildHistoricoPagingLinks(motoId, page, size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<PosicaoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("motos-revisao")]
    [EndpointSummary("Listar posições de motos em revisão")]
    [EndpointDescription("Retorna as posições atuais de todas as motos com status 'Revisão'.")]
    [ProducesResponseType(typeof(PagedResult<Resource<PosicaoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<PosicaoDTO>>>> GetPosicoesDeMotosRevisao(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        var query = _context.Posicoes
            .AsNoTracking()
            .Include(p => p.MotoIdMotoNavigation)
            .Where(p => p.MotoIdMotoNavigation != null && p.MotoIdMotoNavigation.Status.ToLower() == "revisão");

        var total = await query.LongCountAsync();

        var entidades = await query
            .OrderBy(p => p.IdPosicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = entidades.Select(PosicaoMapper.ToDto).ToList();

        var items = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdPosicao }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdPosicao }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdPosicao }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };

            if (dto.MotoId is not null)
            {
                links.Add(new("moto-posicoes",
                    Url.ActionHref(nameof(GetByMotoId), new { motoId = dto.MotoId, page = 1, size = 10 }), "GET"));
                links.Add(new("historico",
                    Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId = dto.MotoId, page = 1, size = 10 }),
                    "GET"));
            }

            return new Resource<PosicaoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);
        var collectionLinks = BuildRevisaoPagingLinks(page, size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<PosicaoDTO>>(items, page, size, total, collectionLinks);
        return Ok(result);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar posição")]
    [EndpointDescription("Cria uma nova posição.")]
    [ProducesResponseType(typeof(Resource<PosicaoDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<PosicaoDTO>>> Create([FromBody] PosicaoDTO dto)
    {
        var ent = PosicaoMapper.ToEntity(dto);
        _context.Posicoes.Add(ent);
        await _context.SaveChangesAsync();

        var resp = PosicaoMapper.ToDto(ent);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = ent.IdPosicao }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id = ent.IdPosicao }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id = ent.IdPosicao }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        if (resp.MotoId is not null)
        {
            links.Add(new("moto-posicoes",
                Url.ActionHref(nameof(GetByMotoId), new { motoId = resp.MotoId, page = 1, size = 10 }), "GET"));
            links.Add(new("historico",
                Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId = resp.MotoId, page = 1, size = 10 }), "GET"));
        }

        return CreatedAtAction(nameof(GetById),
            new { id = ent.IdPosicao },
            new Resource<PosicaoDTO>(resp, links));
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar posição")]
    [EndpointDescription("Atualiza os dados de uma posição existente.")]
    [ProducesResponseType(typeof(Resource<PosicaoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<PosicaoDTO>>> Update([FromRoute] int id, [FromBody] PosicaoDTO dto)
    {
        if (id != dto.IdPosicao) return BadRequest();

        var ent = await _context.Posicoes.FindAsync(id);
        if (ent == null) return NotFound();

        ent.XPos = dto.XPos;
        ent.YPos = dto.YPos;
        ent.DataHora = dto.DataHora;
        ent.MotoIdMoto = dto.MotoId;
        ent.PatioIdPatio = dto.PatioId;

        await _context.SaveChangesAsync();

        var resp = PosicaoMapper.ToDto(ent);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        if (resp.MotoId is not null)
        {
            links.Add(new("moto-posicoes",
                Url.ActionHref(nameof(GetByMotoId), new { motoId = resp.MotoId, page = 1, size = 10 }), "GET"));
            links.Add(new("historico",
                Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId = resp.MotoId, page = 1, size = 10 }), "GET"));
        }

        return Ok(new Resource<PosicaoDTO>(resp, links));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir posição")]
    [EndpointDescription("Remove uma posição do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var ent = await _context.Posicoes.FindAsync(id);
        if (ent == null) return NotFound();

        _context.Posicoes.Remove(ent);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Método auxiliar para rotas com parâmetro no caminho
    private IEnumerable<HateoasLink> BuildMotoPagingLinks(int motoId, int page, int size, int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByMotoId), new { motoId, page, size }), "GET"),
            new("first", Url.ActionHref(nameof(GetByMotoId), new { motoId, page = 1, size }), "GET"),
            new("last",
                Url.ActionHref(nameof(GetByMotoId), new { motoId, page = totalPages > 0 ? totalPages : 1, size }),
                "GET")
        };
        if (page > 1)
            links.Add(new("prev", Url.ActionHref(nameof(GetByMotoId), new { motoId, page = page - 1, size }), "GET"));
        if (totalPages > 0 && page < totalPages)
            links.Add(new("next", Url.ActionHref(nameof(GetByMotoId), new { motoId, page = page + 1, size }), "GET"));
        return links;
    }

    private IEnumerable<HateoasLink> BuildHistoricoPagingLinks(int motoId, int page, int size, int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId, page, size }), "GET"),
            new("first", Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId, page = 1, size }), "GET"),
            new("last",
                Url.ActionHref(nameof(GetHistoricoDaMoto),
                    new { motoId, page = totalPages > 0 ? totalPages : 1, size }), "GET")
        };
        if (page > 1)
            links.Add(new("prev", Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId, page = page - 1, size }),
                "GET"));
        if (totalPages > 0 && page < totalPages)
            links.Add(new("next", Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId, page = page + 1, size }),
                "GET"));
        return links;
    }

    private IEnumerable<HateoasLink> BuildRevisaoPagingLinks(int page, int size, int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetPosicoesDeMotosRevisao), new { page, size }), "GET"),
            new("first", Url.ActionHref(nameof(GetPosicoesDeMotosRevisao), new { page = 1, size }), "GET"),
            new("last",
                Url.ActionHref(nameof(GetPosicoesDeMotosRevisao), new { page = totalPages > 0 ? totalPages : 1, size }),
                "GET")
        };
        if (page > 1)
            links.Add(new("prev", Url.ActionHref(nameof(GetPosicoesDeMotosRevisao), new { page = page - 1, size }),
                "GET"));
        if (totalPages > 0 && page < totalPages)
            links.Add(new("next", Url.ActionHref(nameof(GetPosicoesDeMotosRevisao), new { page = page + 1, size }),
                "GET"));
        return links;
    }
}