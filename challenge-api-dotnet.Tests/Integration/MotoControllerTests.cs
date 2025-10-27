using System.Net;
using System.Net.Http.Json;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Tests.Testing;
using Xunit;

namespace challenge_api_dotnet.Tests.Integration;

[Collection("IntegrationTests")]
public class MotoControllerTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MotoControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSeededMotos()
    {
        _factory.ResetDatabase();

        var response = await _client.GetAsync("/api/v1/motos?page=1&size=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<PagedMotoResponse>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Total);
        Assert.Contains(payload.Items, r => r.Data?.Placa == "ABC1234");
    }

    [Fact]
    public async Task Create_ReturnsCreatedMoto()
    {
        _factory.ResetDatabase();

        var request = new MotoCreateDTO
        {
            Placa = "JKL4567",
            Modelo = "Model X",
            Status = "ativo"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/motos", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var resource = await response.Content.ReadFromJsonAsync<ResourceResponse<MotoDTO>>();
        Assert.NotNull(resource?.Data);
        Assert.Equal("JKL4567", resource!.Data!.Placa);

        var getResponse = await _client.GetAsync($"/api/v1/motos/{resource.Data.IdMoto}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsUpdatedMoto()
    {
        _factory.ResetDatabase();

        var request = new MotoCreateDTO
        {
            IdMoto = 1,
            Placa = "UPDATED1",
            Modelo = "Atualizado",
            Status = "inativo"
        };

        var response = await _client.PutAsJsonAsync("/api/v1/motos/1", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resource = await response.Content.ReadFromJsonAsync<ResourceResponse<MotoDTO>>();
        Assert.NotNull(resource?.Data);
        Assert.Equal("UPDATED1", resource!.Data!.Placa);
        Assert.Equal("inativo", resource.Data.Status);
    }

    [Fact]
    public async Task Delete_RemovesMoto()
    {
        _factory.ResetDatabase();

        var response = await _client.DeleteAsync("/api/v1/motos/2");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync("/api/v1/motos/2");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private sealed class ResourceResponse<T>
    {
        public T? Data { get; set; }
        public List<object>? Links { get; set; }
    }

    private sealed class PagedMotoResponse
    {
        public List<ResourceResponse<MotoDTO>> Items { get; set; } = new();
        public int Page { get; set; }
        public int Size { get; set; }
        public long Total { get; set; }
    }
}
