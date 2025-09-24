namespace challenge_api_dotnet.Hateoas;

public sealed class Resource<T>
{
    //Classe utilitária para hateoas com DTOs
    public T Data { get; }
    public IEnumerable<HateoasLink> Links { get; }

    public Resource(T data, IEnumerable<HateoasLink> links)
    {
        Data = data;
        Links = links;
    }
}