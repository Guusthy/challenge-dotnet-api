using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Authorize]
[Route("api/medicoes")]
[Produces("application/json")]
[Tags("Medições de Posição")]
public class MedicaoPosicaoController : ControllerBase
{
    private readonly IMedicaoPosicaoService _service;
    public MedicaoPosicaoController(IMedicaoPosicaoService service) => _service = service;

    [HttpGet]
    [EndpointSummary("Listar medições de posição")]
    [EndpointDescription("Retorna medições de posição paginadas.")]
    [ProducesResponseType(typeof(PagedResult<Resource<MedicaoPosicaoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MedicaoPosicaoDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var paged = await _service.GetPagedAsync(page, size);

        var items = paged.Items.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMedicao }), "GET"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };
            return new Resource<MedicaoPosicaoDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = Url.PagingLinks(nameof(GetAll), paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result =
            new PagedResult<Resource<MedicaoPosicaoDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter medição por ID")]
    [EndpointDescription("Retorna os dados de uma medição de posição pelo seu identificador, com links.")]
    [ProducesResponseType(typeof(Resource<MedicaoPosicaoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MedicaoPosicaoDTO>>> GetById([FromRoute] int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound();

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
        var paged = await _service.GetByPosicaoIdPagedAsync(id, page, size);

        var items = paged.Items.Select(dto =>
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

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = BuildPosicaoPagingLinks(id, paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result =
            new PagedResult<Resource<MedicaoPosicaoDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
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
        var paged = await _service.GetByMarcadorIdPagedAsync(id, page, size);

        var items = paged.Items.Select(dto =>
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

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = BuildMarcadorPagingLinks(id, paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result =
            new PagedResult<Resource<MedicaoPosicaoDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("contagem/posicao/{id}")]
    [EndpointSummary("Contar medições por posição")]
    [EndpointDescription("Retorna a quantidade de medições vinculadas à posição informada.")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> CountByPosicaoId([FromRoute] int id)
    {
        var count = await _service.CountByPosicaoIdAsync(id);
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
        var created = await _service.CreateAsync(dto);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = created.IdMedicao }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        return CreatedAtAction(nameof(GetById),
            new { id = created.IdMedicao },
            new Resource<MedicaoPosicaoDTO>(created, links));
    }

    // Métodos auxiliares de paginação
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
