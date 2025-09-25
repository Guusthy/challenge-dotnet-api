namespace challenge_api_dotnet.Hateoas;

public sealed class PagedResult<T>
{
    public IEnumerable<T> Items { get; }
    public int Page { get; }
    public int Size { get; }
    public long Total { get; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Size);
    public IReadOnlyList<HateoasLink> Links { get; }

    public PagedResult(IEnumerable<T> items, int page, int size, long total,
        IEnumerable<HateoasLink>? links = null)
    {
        Items = items;
        Page = page;
        Size = size;
        Total = total;
        Links = (links ?? Array.Empty<HateoasLink>()).ToList();
    }
}