using System.Text.Json;
using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Controllers;

[ApiController]
[Route("api/marcadores-moveis")]
[Produces("application/json")]
[Tags("Marcadores ArUco Móveis")]
public class MarcadorArucoMovelController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public MarcadorArucoMovelController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EndpointSummary("Listar marcadores móveis")]
    [EndpointDescription("Retorna marcadores ArUco móveis paginados.")]
    [ProducesResponseType(typeof(PagedResult<Resource<MarcadorArucoMovelDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<MarcadorArucoMovelDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;

        // Define ponto de partida da consulta (ainda não foi ao banco)
        IQueryable<MarcadorArucoMovel> query = _context.MarcadoresArucoMoveis.AsNoTracking();

        var total = await query.LongCountAsync();

        var marcadores = await query
            .OrderBy(m => m.IdMarcadorMovel)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = marcadores.Select(MarcadorArucoMovelMapper.ToDto).ToList();

        // Links por item
        var dtosWithLinks = dtos.Select(dto =>
        {
            var links = new List<HateoasLink>
            {
                new("self", Url.ActionHref(nameof(GetById), new { id = dto.IdMarcadorMovel }), "GET"),
                new("update", Url.ActionHref(nameof(Update), new { id = dto.IdMarcadorMovel }), "PUT"),
                new("delete", Url.ActionHref(nameof(Delete), new { id = dto.IdMarcadorMovel }), "DELETE")
            };

            if (dto.MotoId is not null)
                links.Add(new("moto", Url.ActionHref(nameof(GetByMotoId), new { idMoto = dto.MotoId }), "GET"));

            return new Resource<MarcadorArucoMovelDTO>(dto, links);
        });

        var totalPages = (int)Math.Ceiling((double)total / size);

        // Links da coleção
        var collectionLinks = Url.PagingLinks(nameof(GetAll), page, size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result = new PagedResult<Resource<MarcadorArucoMovelDTO>>(
            dtosWithLinks, page, size, total, collectionLinks);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter marcador móvel por ID")]
    [EndpointDescription("Retorna um marcador ArUco móvel com links de navegação e ações.")]
    [ProducesResponseType(typeof(Resource<MarcadorArucoMovelDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<MarcadorArucoMovelDTO>>> GetById([FromRoute] int id)
    {
        var marcador = await _context.MarcadoresArucoMoveis.FindAsync(id);
        if (marcador == null) return NotFound();

        var dto = MarcadorArucoMovelMapper.ToDto(marcador);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("create", Url.ActionHref(nameof(Create)), "POST")
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
        var marcador = await _context.MarcadoresArucoMoveis
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MotoIdMoto == idMoto);

        if (marcador == null) return NotFound();

        var dto = MarcadorArucoMovelMapper.ToDto(marcador);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByMotoId), new { idMoto }), "GET"),
            new("marcador-by-id", Url.ActionHref(nameof(GetById), new { id = dto.IdMarcadorMovel }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
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
        var marcador = await _context.MarcadoresArucoMoveis
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CodigoAruco.ToLower() == codigoAruco.ToLower());

        if (marcador == null) return NotFound();

        var dto = MarcadorArucoMovelMapper.ToDto(marcador);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetByCodigoAruco), new { codigoAruco }), "GET"),
            new("marcador-by-id", Url.ActionHref(nameof(GetById), new { id = dto.IdMarcadorMovel }), "GET"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
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
        var marcador = MarcadorArucoMovelMapper.ToEntity(dto);
        _context.MarcadoresArucoMoveis.Add(marcador);
        await _context.SaveChangesAsync();

        var response = MarcadorArucoMovelMapper.ToDto(marcador);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = marcador.IdMarcadorMovel }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id = marcador.IdMarcadorMovel }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id = marcador.IdMarcadorMovel }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        return CreatedAtAction(nameof(GetById),
            new { id = marcador.IdMarcadorMovel },
            new Resource<MarcadorArucoMovelDTO>(response, links));
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

        var marcador = await _context.MarcadoresArucoMoveis.FindAsync(id);
        if (marcador == null) return NotFound();

        marcador.CodigoAruco = dto.CodigoAruco;
        marcador.DataInstalacao = dto.DataInstalacao;
        marcador.MotoIdMoto = dto.MotoId;

        await _context.SaveChangesAsync();

        var response = MarcadorArucoMovelMapper.ToDto(marcador);
        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        return Ok(new Resource<MarcadorArucoMovelDTO>(response, links));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir marcador móvel")]
    [EndpointDescription("Remove um marcador ArUco móvel do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var marcador = await _context.MarcadoresArucoMoveis.FindAsync(id);
        if (marcador == null) return NotFound();

        _context.MarcadoresArucoMoveis.Remove(marcador);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}