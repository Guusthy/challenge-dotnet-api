using Asp.Versioning;
using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/usuarios")]
[Produces("application/json")]
[Tags("Usuários")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _service;
    public UsuarioController(IUsuarioService service) => _service = service;

    [HttpGet]
    [EndpointSummary("Listar usuários")]
    [EndpointDescription("Retorna usuários paginados.")]
    [ProducesResponseType(typeof(PagedResult<Resource<UsuarioResponseDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<UsuarioResponseDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var paged = await _service.GetPagedAsync(page, size);

        var items = paged.Items.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdUsuario }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdUsuario }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdUsuario }), "DELETE"),
                new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
                new("create", Url.ActionHref(nameof(Create)), "POST")
            };
            return new Resource<UsuarioResponseDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = Url.PagingLinks(nameof(GetAll), paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result =
            new PagedResult<Resource<UsuarioResponseDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter usuário por ID")]
    [EndpointDescription("Retorna os dados do usuário com links de navegação e ações.")]
    [ProducesResponseType(typeof(Resource<UsuarioResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<UsuarioResponseDTO>>> GetById([FromRoute] int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound();

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("create", Url.ActionHref(nameof(Create)), "POST")
        };

        return Ok(new Resource<UsuarioResponseDTO>(dto, links));
    }

    [HttpGet("email/{email}")]
    [EndpointSummary("Obter usuário por e-mail")]
    [EndpointDescription("Retorna o usuário correspondente ao e-mail informado.")]
    [ProducesResponseType(typeof(Resource<UsuarioResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<UsuarioResponseDTO>>> GetByEmail([FromRoute] string email)
    {
        var dto = await _service.GetByEmailAsync(email);
        if (dto is null) return NotFound();

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByEmail), new { email }), "GET"),
            new("by-id", Url.ActionHref(nameof(GetById), new { id = dto.IdUsuario }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("create", Url.ActionHref(nameof(Create)), "POST")
        };

        return Ok(new Resource<UsuarioResponseDTO>(dto, links));
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Criar usuário")]
    [EndpointDescription("Cria um novo usuário.")]
    [ProducesResponseType(typeof(Resource<UsuarioResponseDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<UsuarioResponseDTO>>> Create([FromBody] UsuarioCreateDTO dto)
    {
        var created = await _service.CreateAsync(dto);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = created.IdUsuario }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id = created.IdUsuario }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id = created.IdUsuario }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        return CreatedAtAction(nameof(GetById),
            new { id = created.IdUsuario },
            new Resource<UsuarioResponseDTO>(created, links));
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [EndpointSummary("Atualizar usuário")]
    [EndpointDescription("Atualiza os dados de um usuário existente.")]
    [ProducesResponseType(typeof(Resource<UsuarioResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<UsuarioResponseDTO>>> Update([FromRoute] int id,
        [FromBody] UsuarioCreateDTO dto)
    {
        if (id != dto.IdUsuario) return BadRequest();

        var updated = await _service.UpdateAsync(id, dto);
        if (updated is null) return NotFound();

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        return Ok(new Resource<UsuarioResponseDTO>(updated, links));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir usuário")]
    [EndpointDescription("Remove um usuário do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
        => (await _service.DeleteAsync(id)) ? NoContent() : NotFound();
}
