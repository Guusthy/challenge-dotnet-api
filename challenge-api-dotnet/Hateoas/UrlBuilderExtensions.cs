using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace challenge_api_dotnet.Hateoas;

public static class UrlBuilderExtensions
{
    // Padronização da geração de URLs
    public static string ActionHref(this IUrlHelper url, string action, object? routeValues = null)
    {
        var values = routeValues is null
            ? new RouteValueDictionary()
            : new RouteValueDictionary(routeValues);

        if (!values.ContainsKey("version"))
        {
            var requestedVersion = url.ActionContext.HttpContext.GetRequestedApiVersion()?.ToString();
            values["version"] = requestedVersion ?? "1.0";
        }

        return url.Action(action, values) ?? string.Empty;
    }

    public static IEnumerable<HateoasLink> PagingLinks(this IUrlHelper url, string listAction, int page, int size,
        int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self", url.ActionHref(listAction, new { page, size }), "GET"),
            new("first", url.ActionHref(listAction, new { page = 1, size }), "GET"),
            new("last", url.ActionHref(listAction, new { page = totalPages > 0 ? totalPages : 1, size }), "GET")
        };

        if (page > 1)
            links.Add(new("prev", url.ActionHref(listAction, new { page = page - 1, size }), "GET"));

        if (totalPages > 0 && page < totalPages)
            links.Add(new("next", url.ActionHref(listAction, new { page = page + 1, size }), "GET"));

        return links;
    }

    public static HateoasLink CreateLink(this IUrlHelper url,
        string rel, string actionName, object? routeValues = null, string method = "GET")
        => new(rel, url.ActionHref(actionName, routeValues), method);

    /// Gera links CRUD padrão para um item (self/update/delete) + extras opcionais
    public static IEnumerable<HateoasLink> CrudLinks(this IUrlHelper url,
        string getByIdAction, string updateAction, string deleteAction, object idRouteValues,
        IEnumerable<HateoasLink>? extra = null)
    {
        var links = new List<HateoasLink>
        {
            url.CreateLink("self", getByIdAction, idRouteValues, "GET"),
            url.CreateLink("update", updateAction, idRouteValues, "PUT"),
            url.CreateLink("delete", deleteAction, idRouteValues, "DELETE"),
        };
        if (extra is not null) links.AddRange(extra);
        return links;
    }

    /// Converte um DTO em Resource<T> com CRUD + extras
    public static Resource<TDto> ToResource<TDto>(this IUrlHelper url,
        TDto dto, object idRouteValues,
        string getByIdAction, string updateAction, string deleteAction,
        IEnumerable<HateoasLink>? extra = null)
    {
        var links = url.CrudLinks(getByIdAction, updateAction, deleteAction, idRouteValues, extra);
        return new Resource<TDto>(dto, links);
    }

    /// Links da coleção (paginações) + create opcional
    public static IEnumerable<HateoasLink> CollectionLinks(this IUrlHelper url,
        string listAction, int page, int size, int totalPages, string? createAction = null)
    {
        var links = url.PagingLinks(listAction, page, size, totalPages).ToList();
        if (createAction is not null)
            links.Add(new HateoasLink("create", url.ActionHref(createAction), "POST"));
        return links;
    }
}
