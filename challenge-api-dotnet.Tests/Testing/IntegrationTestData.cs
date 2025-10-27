using challenge_api_dotnet.Data;
using challenge_api_dotnet.Models;

namespace challenge_api_dotnet.Tests.Testing;

public static class IntegrationTestData
{
    public const int TrainingMedicoesCount = 5;

    public static void Seed(ApplicationDbContext context)
    {
        context.Posicoes.RemoveRange(context.Posicoes);
        context.MarcadoresFixos.RemoveRange(context.MarcadoresFixos);
        context.MedicoesPosicoes.RemoveRange(context.MedicoesPosicoes);

        var posicao = new Posicao
        {
            IdPosicao = 1,
            XPos = 1.2m,
            YPos = 2.2m
        };

        var marcador = new MarcadorFixo
        {
            IdMarcadorArucoFixo = 1,
            XPos = 4.8m,
            YPos = 3.1m
        };

        context.Posicoes.Add(posicao);
        context.MarcadoresFixos.Add(marcador);

        for (var i = 0; i < TrainingMedicoesCount; i++)
        {
            context.MedicoesPosicoes.Add(new MedicaoPosicao
            {
                IdMedicao = i + 1,
                DistanciaM = 1.5m + i,
                PosicaoIdPosicao = posicao.IdPosicao,
                MarcadorFixoIdMarcadorArucoFixo = marcador.IdMarcadorArucoFixo
            });
        }

        context.SaveChanges();
    }
}
