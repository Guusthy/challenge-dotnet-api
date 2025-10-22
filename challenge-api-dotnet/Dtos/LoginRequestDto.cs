namespace challenge_api_dotnet.Dtos;

public sealed class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
