namespace challenge_api_dotnet.Hateoas;

public sealed class PagedResult<T>
{
    public IEnumerable<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public long TotalItems { get; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public IEnumerable<Link> Links { get; }

    public PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, long totalItems, IEnumerable<Link> links)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
        Links = links;
    }
}