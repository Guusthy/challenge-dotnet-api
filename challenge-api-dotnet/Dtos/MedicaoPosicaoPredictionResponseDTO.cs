namespace challenge_api_dotnet.Dtos;

public class MedicaoPosicaoPredictionResponseDTO
{
    public int PosicaoId { get; set; }
    public int MarcadorFixoId { get; set; }
    public float PredictedDistance { get; set; }
    public int TrainingSampleCount { get; set; }
}
