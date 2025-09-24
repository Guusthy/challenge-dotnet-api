namespace challenge_api_dotnet.Hateoas;

public sealed class Link
{
    public string Rel { get; }
    public string Href { get; }
    public string Method { get; }

    public Link(string rel, string href, string method)
    {
        Rel = rel;
        Href = href;
        Method = method;
    }
}