namespace challenge_api_dotnet.Dtos;

public class UsuarioCreateDTO
{
    public int IdUsuario { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
    public string Status { get; set; }
    public string Tipo { get; set; }
    public int PatioId { get; set; }

}
