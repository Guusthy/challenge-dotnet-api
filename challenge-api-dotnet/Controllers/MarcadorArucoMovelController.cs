using System.Text.Json;
using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/marcadores-moveis")]
[Produces("application/json")]
[Tags("Marcadores ArUco Móveis")]
public class MarcadorArucoMovelController : ControllerBase
{
    private readonly IMarcadorArucoMovelService _service;
    public MarcadorArucoMovelController(IMarcadorArucoMovelService service) => _service = service;

    [HttpGet]
    [EndpointSummary("Listar marcadores móveis")]
    [EndpointDescription("Retorna marcadores ArUco móveis paginados.")]
    [ProducesResponseType(typeof(PagedResult<Resource<MarcadorArucoMovelDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MarcadorArucoMovelDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var pagedResult = await _service.GetPagedAsync(page, size);

        // HATEOAS por item
        var itemResources = pagedResult.Items.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMarcadorMovel }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdMarcadorMovel }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdMarcadorMovel }), "DELETE"),
            };

            if (dto.MotoId is not null)
                links.Add(new("moto", Url.ActionHref(nameof(GetByMotoId), new { idMoto = dto.MotoId }), "GET"));

            return new Resource<MarcadorArucoMovelDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)pagedResult.Total / pagedResult.Size);
        // Cria os links HATEOAS da coleção
        var collectionLinks = Url.PagingLinks(nameof(GetAll), pagedResult.Page, pagedResult.Size, totalPages).ToList();

        var result = new PagedResult<Resource<MarcadorArucoMovelDTO>>(
            itemResources, pagedResult.Page, pagedResult.Size, pagedResult.Total, collectionLinks);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter marcador móvel por ID")]
    [EndpointDescription("Retorna um marcador ArUco móvel com links de navegação e ações.")]
    [ProducesResponseType(typeof(Resource<MarcadorArucoMovelDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MarcadorArucoMovelDTO>>> GetById([FromRoute] int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound();

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("create", Url.ActionHref(nameof(Create)), "POST"),
        };

        if (dto.MotoId is not null)
            links.Add(new("moto", Url.ActionHref(nameof(GetByMotoId), new { idMoto = dto.MotoId }), "GET"));

        return Ok(new Resource<MarcadorArucoMovelDTO>(dto, links));
    }

    [HttpGet("moto/{idMoto}")]
    [EndpointSummary("Obter marcador por moto")]
    [EndpointDescription("Retorna o marcador ArUco móvel vinculado a uma moto específica.")]
    [ProducesResponseType(typeof(Resource<MarcadorArucoMovelDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MarcadorArucoMovelDTO>>> GetByMotoId([FromRoute] int idMoto)
    {
        var dto = await _service.GetByMotoIdAsync(idMoto);
        if (dto is null) return NotFound();

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByMotoId), new { idMoto }), "GET"),
            new("marcador-by-id", Url.ActionHref(nameof(GetById), new { id = dto.IdMarcadorMovel }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
        };

        return Ok(new Resource<MarcadorArucoMovelDTO>(dto, links));
    }

    [HttpGet("busca")]
    [EndpointSummary("Buscar marcador por código ArUco")]
    [EndpointDescription("Retorna um marcador ArUco móvel filtrado pelo código (Ex: MOVEL_002).")]
    [ProducesResponseType(typeof(Resource<MarcadorArucoMovelDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MarcadorArucoMovelDTO>>> GetByCodigoAruco([FromQuery] string codigoAruco)
    {
        var dto = await _service.GetByCodigoArucoAsync(codigoAruco);
        if (dto is null) return NotFound();

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByCodigoAruco), new { codigoAruco }), "GET"),
            new("marcador-by-id", Url.ActionHref(nameof(GetById), new { id = dto.IdMarcadorMovel }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
        };

        return Ok(new Resource<MarcadorArucoMovelDTO>(dto, links));
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar marcador móvel")]
    [EndpointDescription("Cria um novo marcador ArUco móvel.")]
    [ProducesResponseType(typeof(Resource<MarcadorArucoMovelDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<MarcadorArucoMovelDTO>>> Create([FromBody] MarcadorArucoMovelDTO dto)
    {
        var created = await _service.CreateAsync(dto);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = created.IdMarcadorMovel }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id = created.IdMarcadorMovel }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id = created.IdMarcadorMovel }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
        };

        return CreatedAtAction(nameof(GetById),
            new { id = created.IdMarcadorMovel },
            new Resource<MarcadorArucoMovelDTO>(created, links));
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar marcador móvel")]
    [EndpointDescription("Atualiza os dados de um marcador ArUco móvel existente.")]
    [ProducesResponseType(typeof(Resource<MarcadorArucoMovelDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MarcadorArucoMovelDTO>>> Update([FromRoute] int id,
        [FromBody] MarcadorArucoMovelDTO dto)
    {
        if (id != dto.IdMarcadorMovel) return BadRequest();

        var updated = await _service.UpdateAsync(id, dto);
        if (updated is null) return NotFound();

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
        };

        return Ok(new Resource<MarcadorArucoMovelDTO>(updated, links));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir marcador móvel")]
    [EndpointDescription("Remove um marcador ArUco móvel do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
        => (await _service.DeleteAsync(id)) ? NoContent() : NotFound();
}