using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Tests.Testing;
using Xunit;

namespace challenge_api_dotnet.Tests.Integration;

[Collection("IntegrationTests")]
public class AuthControllerTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsCreatedResourceWithToken()
    {
        _factory.ResetDatabase();

        var request = new UsuarioCreateDTO
        {
            Nome = "Novo Usuario",
            Email = "novo.auth@example.com",
            Senha = "SenhaSegura!1",
            Status = "ativo",
            Tipo = "admin",
            PatioId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ResourceResponse<AuthResponseDto>>();
        Assert.NotNull(payload?.Data);
        Assert.Equal("novo.auth@example.com", payload!.Data!.Usuario.Email);
        Assert.False(string.IsNullOrWhiteSpace(payload.Data.Token));
    }

    [Fact]
    public async Task Login_ReturnsTokenForExistingUser()
    {
        _factory.ResetDatabase();

        var registerRequest = new UsuarioCreateDTO
        {
            Nome = "Login Usuario",
            Email = "login.auth@example.com",
            Senha = "SenhaMuitoForte!2",
            Status = "ativo",
            Tipo = "admin",
            PatioId = 1
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginRequest = new LoginRequestDto
        {
            Email = "login.auth@example.com",
            Senha = "SenhaMuitoForte!2"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var payload = await loginResponse.Content.ReadFromJsonAsync<ResourceResponse<AuthResponseDto>>();
        Assert.NotNull(payload?.Data);
        Assert.Equal("login.auth@example.com", payload!.Data!.Usuario.Email);
        Assert.False(string.IsNullOrWhiteSpace(payload.Data.Token));
    }

    private sealed class ResourceResponse<T>
    {
        public T? Data { get; set; }
        public List<object>? Links { get; set; }
    }
}
