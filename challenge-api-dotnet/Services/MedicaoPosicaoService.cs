using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace challenge_api_dotnet.Services;

public sealed class MedicaoPosicaoService(ApplicationDbContext db) : IMedicaoPosicaoService
{
    private readonly ApplicationDbContext _db = db;

    private static (int page, int size) Normalize(int page, int size)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;
        return (page, size);
    }

    public async Task<PagedResult<MedicaoPosicaoDTO>> GetPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.MedicoesPosicoes.AsNoTracking();
        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMedicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MedicaoPosicaoMapper.ToDto).ToList();
        return new PagedResult<MedicaoPosicaoDTO>(dtos, page, size, total);
    }

    public async Task<MedicaoPosicaoDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.MedicoesPosicoes.FindAsync(id);
        return entity is null ? null : MedicaoPosicaoMapper.ToDto(entity);
    }

    public async Task<PagedResult<MedicaoPosicaoDTO>> GetByPosicaoIdPagedAsync(int posicaoId, int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.MedicoesPosicoes
            .AsNoTracking()
            .Where(m => m.PosicaoIdPosicao == posicaoId);

        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMedicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MedicaoPosicaoMapper.ToDto).ToList();
        return new PagedResult<MedicaoPosicaoDTO>(dtos, page, size, total);
    }

    public async Task<PagedResult<MedicaoPosicaoDTO>> GetByMarcadorIdPagedAsync(int marcadorFixoId, int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.MedicoesPosicoes
            .AsNoTracking()
            .Where(m => m.MarcadorFixoIdMarcadorArucoFixo == marcadorFixoId);

        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(m => m.IdMedicao)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(MedicaoPosicaoMapper.ToDto).ToList();
        return new PagedResult<MedicaoPosicaoDTO>(dtos, page, size, total);
    }

    public async Task<int> CountByPosicaoIdAsync(int posicaoId)
    {
        return await _db.MedicoesPosicoes
            .AsNoTracking()
            .CountAsync(m => m.PosicaoIdPosicao == posicaoId);
    }

    public async Task<MedicaoPosicaoDTO> CreateAsync(MedicaoPosicaoDTO dto)
    {
        var entity = MedicaoPosicaoMapper.ToEntity(dto);
        _db.MedicoesPosicoes.Add(entity);
        await _db.SaveChangesAsync();
        return MedicaoPosicaoMapper.ToDto(entity);
    }

    public async Task<MedicaoPosicaoPredictionResponseDTO> PredictDistanceAsync(MedicaoPosicaoPredictionRequestDTO request)
    {
        var posicao = await _db.Posicoes
                         .AsNoTracking()
                         .FirstOrDefaultAsync(p => p.IdPosicao == request.PosicaoId)
                     ?? throw new KeyNotFoundException($"Posição {request.PosicaoId} não encontrada.");

        var marcador = await _db.MarcadoresFixos
                          .AsNoTracking()
                          .FirstOrDefaultAsync(m => m.IdMarcadorArucoFixo == request.MarcadorFixoId)
                      ?? throw new KeyNotFoundException($"Marcador fixo {request.MarcadorFixoId} não encontrado.");

        var trainingData = await (from med in _db.MedicoesPosicoes.AsNoTracking()
                                  where med.DistanciaM != null
                                        && med.PosicaoIdPosicao != null
                                        && med.MarcadorFixoIdMarcadorArucoFixo != null
                                  join pos in _db.Posicoes.AsNoTracking()
                                      on med.PosicaoIdPosicao equals pos.IdPosicao
                                  join marker in _db.MarcadoresFixos.AsNoTracking()
                                      on med.MarcadorFixoIdMarcadorArucoFixo equals marker.IdMarcadorArucoFixo
                                  select new MedicaoModelInput
                                  {
                                      PosicaoX = ToFloat(pos.XPos),
                                      PosicaoY = ToFloat(pos.YPos),
                                      MarcadorX = ToFloat(marker.XPos),
                                      MarcadorY = ToFloat(marker.YPos),
                                      Label = ToFloat(med.DistanciaM)
                                  }).ToListAsync();

        if (trainingData.Count < 5)
        {
            throw new InvalidOperationException(
                "Dados insuficientes para treinar o modelo de predição de distância.");
        }

        var mlContext = new MLContext(seed: 42);
        var dataView = mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = mlContext.Transforms.Concatenate("Features",
                                 nameof(MedicaoModelInput.PosicaoX),
                                 nameof(MedicaoModelInput.PosicaoY),
                                 nameof(MedicaoModelInput.MarcadorX),
                                 nameof(MedicaoModelInput.MarcadorY))
            .Append(mlContext.Regression.Trainers.Sdca(
                labelColumnName: "Label",
                featureColumnName: "Features"));

        var model = pipeline.Fit(dataView);
        var predictionEngine = mlContext.Model.CreatePredictionEngine<MedicaoModelInput, MedicaoModelOutput>(model);

        var input = new MedicaoModelInput
        {
            PosicaoX = ToFloat(posicao.XPos),
            PosicaoY = ToFloat(posicao.YPos),
            MarcadorX = ToFloat(marcador.XPos),
            MarcadorY = ToFloat(marcador.YPos),
            Label = 0f
        };

        var prediction = predictionEngine.Predict(input);

        return new MedicaoPosicaoPredictionResponseDTO
        {
            PosicaoId = request.PosicaoId,
            MarcadorFixoId = request.MarcadorFixoId,
            PredictedDistance = prediction.Score,
            TrainingSampleCount = trainingData.Count
        };
    }

    private static float ToFloat(decimal? value) => value.HasValue ? (float)value.Value : 0f;

    private sealed class MedicaoModelInput
    {
        [ColumnName("Label")]
        public float Label { get; set; }
        public float PosicaoX { get; set; }
        public float PosicaoY { get; set; }
        public float MarcadorX { get; set; }
        public float MarcadorY { get; set; }
    }

    private sealed class MedicaoModelOutput
    {
        public float Score { get; set; }
    }
}
