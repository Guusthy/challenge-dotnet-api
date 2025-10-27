using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Models;
using challenge_api_dotnet.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace challenge_api_dotnet.Tests.Services;

public class MedicaoPosicaoServiceTests
{
    [Fact]
    public async Task PredictDistanceAsync_WithValidData_ReturnsPrediction()
    {
        await using var context = CreateContext();
        SeedMeasurements(context, measurementCount: 5);

        var service = new MedicaoPosicaoService(context);
        var request = new MedicaoPosicaoPredictionRequestDTO { PosicaoId = 1, MarcadorFixoId = 1 };

        var response = await service.PredictDistanceAsync(request);

        Assert.NotNull(response);
        Assert.Equal(request.PosicaoId, response.PosicaoId);
        Assert.Equal(request.MarcadorFixoId, response.MarcadorFixoId);
        Assert.Equal(5, response.TrainingSampleCount);
        Assert.False(float.IsNaN(response.PredictedDistance));
    }

    [Fact]
    public async Task PredictDistanceAsync_WithInsufficientSamples_ThrowsInvalidOperation()
    {
        await using var context = CreateContext();
        SeedMeasurements(context, measurementCount: 2);

        var service = new MedicaoPosicaoService(context);
        var request = new MedicaoPosicaoPredictionRequestDTO { PosicaoId = 1, MarcadorFixoId = 1 };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.PredictDistanceAsync(request));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedMeasurements(ApplicationDbContext context, int measurementCount)
    {
        var posicao = new Posicao { IdPosicao = 1, XPos = 1.5m, YPos = 2.5m };
        var marcador = new MarcadorFixo { IdMarcadorArucoFixo = 1, XPos = 3.5m, YPos = 4.5m };
        context.Posicoes.Add(posicao);
        context.MarcadoresFixos.Add(marcador);

        for (var i = 0; i < measurementCount; i++)
        {
            context.MedicoesPosicoes.Add(new MedicaoPosicao
            {
                IdMedicao = i + 1,
                DistanciaM = 1.1m + i,
                PosicaoIdPosicao = posicao.IdPosicao,
                MarcadorFixoIdMarcadorArucoFixo = marcador.IdMarcadorArucoFixo
            });
        }

        context.SaveChanges();
    }
}
