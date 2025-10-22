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
[Route("api/v{version:apiVersion}/posicoes")]
[Produces("application/json")]
[Tags("Posições")]
public class PosicaoController : ControllerBase
{
    private readonly IPosicaoService _service;
    public PosicaoController(IPosicaoService service) => _service = service;

    [HttpGet]
    [EndpointSummary("Listar posições")]
    [EndpointDescription("Retorna posições paginadas.")]
    [ProducesResponseType(typeof(PagedResult<Resource<PosicaoDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Resource<PosicaoDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var paged = await _service.GetPagedAsync(page, size);

        var items = paged.Items.Select(dto =>
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

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = Url.PagingLinks(nameof(GetAll), paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("create", Url.ActionHref(nameof(Create)), "POST"));

        var result = new PagedResult<Resource<PosicaoDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointSummary("Obter posição por ID")]
    [EndpointDescription("Retorna os dados da posição especificada, com links.")]
    [ProducesResponseType(typeof(Resource<PosicaoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<PosicaoDTO>>> GetById([FromRoute] int id)
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
        var paged = await _service.GetByMotoIdPagedAsync(motoId, page, size);

        var items = paged.Items.Select(dto =>
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

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = BuildMotoPagingLinks(motoId, paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<PosicaoDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
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
        var paged = await _service.GetHistoricoByMotoPagedAsync(motoId, page, size);

        var items = paged.Items.Select(dto =>
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

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = BuildHistoricoPagingLinks(motoId, paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<PosicaoDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
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
        var paged = await _service.GetPosicoesDeMotosRevisaoPagedAsync(page, size);

        var items = paged.Items.Select(dto =>
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

        var totalPages = (int)Math.Ceiling((double)paged.Total / paged.Size);
        var collectionLinks = BuildRevisaoPagingLinks(paged.Page, paged.Size, totalPages).ToList();
        collectionLinks.Add(new("list-all", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET"));

        var result = new PagedResult<Resource<PosicaoDTO>>(items, paged.Page, paged.Size, paged.Total, collectionLinks);
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
        var created = await _service.CreateAsync(dto);

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id = created.IdPosicao }), "GET"),
            new("update", Url.ActionHref(nameof(Update), new { id = created.IdPosicao }), "PUT"),
            new("delete", Url.ActionHref(nameof(Delete), new { id = created.IdPosicao }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        if (created.MotoId is not null)
        {
            links.Add(new("moto-posicoes",
                Url.ActionHref(nameof(GetByMotoId), new { motoId = created.MotoId, page = 1, size = 10 }), "GET"));
            links.Add(new("historico",
                Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId = created.MotoId, page = 1, size = 10 }),
                "GET"));
        }

        return CreatedAtAction(nameof(GetById),
            new { id = created.IdPosicao },
            new Resource<PosicaoDTO>(created, links));
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

        var updated = await _service.UpdateAsync(id, dto);
        if (updated is null) return NotFound();

        var links = new List<HateoasLink>
        {
            new("self", Url.ActionHref(nameof(GetById), new { id }), "GET"),
            new("delete", Url.ActionHref(nameof(Delete), new { id }), "DELETE"),
            new("list", Url.ActionHref(nameof(GetAll), new { page = 1, size = 10 }), "GET")
        };

        if (updated.MotoId is not null)
        {
            links.Add(new("moto-posicoes",
                Url.ActionHref(nameof(GetByMotoId), new { motoId = updated.MotoId, page = 1, size = 10 }), "GET"));
            links.Add(new("historico",
                Url.ActionHref(nameof(GetHistoricoDaMoto), new { motoId = updated.MotoId, page = 1, size = 10 }),
                "GET"));
        }

        return Ok(new Resource<PosicaoDTO>(updated, links));
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Excluir posição")]
    [EndpointDescription("Remove uma posição do sistema.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
        => (await _service.DeleteAsync(id)) ? NoContent() : NotFound();

    // Métodos helpers de paginação com rota parametrizada
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
