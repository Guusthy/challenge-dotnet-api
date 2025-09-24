using Microsoft.AspNetCore.Mvc;

namespace challenge_api_dotnet.Hateoas;

public static class UrlBuilderExtensions
{
    // Padronização da geração de URLs
    public static string ActionHref(this IUrlHelper url, string action, object? routeValues = null)
        => url.Action(action, routeValues) ?? string.Empty;

    public static IEnumerable<HateoasLink> PagingLinks(this IUrlHelper url, string listAction, int page, int size, int totalPages)
    {
        var links = new List<HateoasLink>
        {
            new("self",  url.ActionHref(listAction, new { page, size }), "GET"),
            new("first", url.ActionHref(listAction, new { page = 1, size }), "GET"),
            new("last",  url.ActionHref(listAction, new { page = totalPages > 0 ? totalPages : 1, size }), "GET")
        };

        if (page > 1)
            links.Add(new("prev", url.ActionHref(listAction, new { page = page - 1, size }), "GET"));

        if (totalPages > 0 && page < totalPages)
            links.Add(new("next", url.ActionHref(listAction, new { page = page + 1, size }), "GET"));

        return links;
    }
}