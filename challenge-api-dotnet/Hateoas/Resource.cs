namespace challenge_api_dotnet.Hateoas;

public sealed class Resource<T>
{
    //Classe utilitária para hateoas com DTOs
    public T Dto { get; }
    public IEnumerable<Link> Links { get; }

    public Resource(T dto, IEnumerable<Link> links)
    {
        Dto = dto;
        Links = links;
    }
}