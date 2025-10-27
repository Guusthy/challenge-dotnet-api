using challenge_api_dotnet.Data;
using challenge_api_dotnet.Models;
using Microsoft.AspNetCore.Identity;

namespace challenge_api_dotnet.Tests.Testing;

public static class IntegrationTestData
{
    public const int TrainingMedicoesCount = 5;

    public static void Seed(ApplicationDbContext context)
    {
        context.MedicoesPosicoes.RemoveRange(context.MedicoesPosicoes);
        context.Posicoes.RemoveRange(context.Posicoes);
        context.MarcadoresFixos.RemoveRange(context.MarcadoresFixos);
        context.Usuarios.RemoveRange(context.Usuarios);
        context.Motos.RemoveRange(context.Motos);
        context.Patios.RemoveRange(context.Patios);

        var patio1 = new Patio { IdPatio = 1, Nome = "Patio Central", Localizacao = "Centro", Descricao = "Principal" };
        var patio2 = new Patio { IdPatio = 2, Nome = "Patio Secundario", Localizacao = "Zona Sul", Descricao = "Secundario" };
        context.Patios.AddRange(patio1, patio2);

        var moto1 = new Moto { IdMoto = 1, Placa = "ABC1234", Modelo = "CG 160", Status = "ativo", DataCadastro = DateTime.UtcNow.AddDays(-2) };
        var moto2 = new Moto { IdMoto = 2, Placa = "DEF5678", Modelo = "Factor 150", Status = "inativo", DataCadastro = DateTime.UtcNow.AddDays(-1) };
        context.Motos.AddRange(moto1, moto2);

        var posicao1 = new Posicao
        {
            IdPosicao = 1,
            XPos = 1.2m,
            YPos = 2.2m,
            MotoIdMoto = moto1.IdMoto,
            PatioIdPatio = patio1.IdPatio
        };

        var posicao2 = new Posicao
        {
            IdPosicao = 2,
            XPos = 3.4m,
            YPos = 1.8m,
            MotoIdMoto = moto2.IdMoto,
            PatioIdPatio = patio1.IdPatio
        };

        var marcador = new MarcadorFixo
        {
            IdMarcadorArucoFixo = 1,
            XPos = 4.8m,
            YPos = 3.1m,
            PatioIdPatio = patio1.IdPatio
        };

        var usuario = new Usuario
        {
            IdUsuario = 1,
            Nome = "Carlos",
            Email = "carlos@example.com",
            PatioIdPatio = patio1.IdPatio
        };

        var passwordHasher = new PasswordHasher<Usuario>();
        usuario.Senha = passwordHasher.HashPassword(usuario, "SenhaSegura!1");

        context.Posicoes.AddRange(posicao1, posicao2);
        context.MarcadoresFixos.Add(marcador);
        context.Usuarios.Add(usuario);

        for (var i = 0; i < TrainingMedicoesCount; i++)
        {
            context.MedicoesPosicoes.Add(new MedicaoPosicao
            {
                IdMedicao = i + 1,
                DistanciaM = 1.5m + i,
                PosicaoIdPosicao = posicao1.IdPosicao,
                MarcadorFixoIdMarcadorArucoFixo = marcador.IdMarcadorArucoFixo
            });
        }

        context.SaveChanges();
    }
}
