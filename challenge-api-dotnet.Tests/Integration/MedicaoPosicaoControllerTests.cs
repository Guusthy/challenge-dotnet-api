using System.Net;
using System.Net.Http.Json;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Tests.Testing;
using Xunit;

namespace challenge_api_dotnet.Tests.Integration;

[Collection("IntegrationTests")]
public class MedicaoPosicaoControllerTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MedicaoPosicaoControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PredictDistance_ReturnsOk()
    {
        _factory.ResetDatabase();

        var request = new MedicaoPosicaoPredictionRequestDTO
        {
            PosicaoId = 1,
            MarcadorFixoId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/v1/medicoes/predicao", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<MedicaoPosicaoPredictionResponseDTO>();
        Assert.NotNull(payload);
        Assert.Equal(request.PosicaoId, payload!.PosicaoId);
        Assert.Equal(request.MarcadorFixoId, payload.MarcadorFixoId);
        Assert.Equal(IntegrationTestData.TrainingMedicoesCount, payload.TrainingSampleCount);
    }

    [Fact]
    public async Task PredictDistance_WithUnknownPosicao_ReturnsNotFound()
    {
        _factory.ResetDatabase();

        var request = new MedicaoPosicaoPredictionRequestDTO
        {
            PosicaoId = 999,
            MarcadorFixoId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/v1/medicoes/predicao", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
