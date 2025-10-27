using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Tests.Testing;
using Xunit;

namespace challenge_api_dotnet.Tests.Integration;

[Collection("IntegrationTests")]
public class PatioControllerTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PatioControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSeededPatios()
    {
        _factory.ResetDatabase();

        var response = await _client.GetAsync("/api/v1/patios?page=1&size=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<PagedPatioResponse>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Total);
        Assert.Contains(payload.Items, r => r.Data?.Nome == "Patio Central");
    }

    [Fact]
    public async Task Create_ReturnsCreatedPatio()
    {
        _factory.ResetDatabase();

        var request = new PatioDTO
        {
            Nome = "Novo Patio",
            Localizacao = "Zona Norte",
            Descricao = "Coberto"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/patios", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var resource = await response.Content.ReadFromJsonAsync<ResourceResponse<PatioDTO>>();
        Assert.NotNull(resource?.Data);
        Assert.Equal("Novo Patio", resource!.Data!.Nome);

        var getResponse = await _client.GetAsync($"/api/v1/patios/{resource.Data.IdPatio}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsUpdatedPatio()
    {
        _factory.ResetDatabase();

        var request = new PatioDTO
        {
            IdPatio = 1,
            Nome = "Patio Atualizado",
            Localizacao = "Centro Expandido",
            Descricao = "Atualizado"
        };

        var response = await _client.PutAsJsonAsync("/api/v1/patios/1", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resource = await response.Content.ReadFromJsonAsync<ResourceResponse<PatioDTO>>();
        Assert.NotNull(resource?.Data);
        Assert.Equal("Patio Atualizado", resource!.Data!.Nome);
        Assert.Equal("Centro Expandido", resource.Data.Localizacao);
    }

    [Fact]
    public async Task Delete_RemovesPatio()
    {
        _factory.ResetDatabase();

        var response = await _client.DeleteAsync("/api/v1/patios/2");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync("/api/v1/patios/2");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private sealed class ResourceResponse<T>
    {
        public T? Data { get; set; }
        public List<object>? Links { get; set; }
    }

    private sealed class PagedPatioResponse
    {
        public List<ResourceResponse<PatioDTO>> Items { get; set; } = new();
        public int Page { get; set; }
        public int Size { get; set; }
        public long Total { get; set; }
    }
}
