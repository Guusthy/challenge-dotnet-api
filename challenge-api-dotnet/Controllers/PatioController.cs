using Asp.Versioning;
using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/patios")]
[Produces("application/json")]
[Tags("Pátios")]
public class PatioController : ControllerBase
{
    private readonly IPatioService _service;
    public PatioController(IPatioService service) => _service = service;

    [HttpGet]
    [EndpointSummary("Listar pátios")]
    [EndpointDescription("Retorna pátios paginados.")]
    [ProducesResponseType(typeof(PagedResult<Resource<PatioDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<PatioDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var paged = await _service.GetPagedAsync(page, size);

        var items = paged.Items.Select(dto =>
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

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = Url.PagingLinks(nameof(GetAll), paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result = new PagedResult<Resource<PatioDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter pátio por ID")]
    [EndpointDescription("Retorna os dados do pátio com links de navegação e ações.")]
    [ProducesResponseType(typeof(Resource<PatioDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<PatioDTO>>> GetById([FromRoute] int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound();

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
        var paged = await _service.GetWithRelationsPagedAsync(page, size);

        var items = paged.Items.Select(dto =>
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

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = Url.PagingLinks(nameof(GetPatiosComMotos), paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<PatioDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}/motos")]
    [EndpointSummary("Listar motos de um pátio")]
    [EndpointDescription("Retorna todas as motos atualmente associadas ao pátio informado.")]
    [ProducesResponseType(typeof(List<MotoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MotoDTO>>> GetMotosPorPatio([FromRoute] int id)
        => await _service.GetMotosByPatioAsync(id);

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar pátio")]
    [EndpointDescription("Cria um novo pátio.")]
    [ProducesResponseType(typeof(Resource<PatioDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<PatioDTO>>> Create([FromBody] PatioDTO dto)
    {
        var created = await _service.CreateAsync(dto);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = created.IdPatio }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id = created.IdPatio }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id = created.IdPatio }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("motos", Url.ActionHref(nameof(GetMotosPorPatio), new { id = created.IdPatio }), "GET")
        };

        return CreatedAtAction(nameof(GetById),
            new { id = created.IdPatio },
            new Resource<PatioDTO>(created, links));
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

        var updated = await _service.UpdateAsync(id, dto);
        if (updated is null) return NotFound();

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("motos", Url.ActionHref(nameof(GetMotosPorPatio), new { id }), "GET")
        };

        return Ok(new Resource<PatioDTO>(updated, links));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir pátio")]
    [EndpointDescription("Remove um pátio do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
        => (await _service.DeleteAsync(id)) ? NoContent() : NotFound();
}
