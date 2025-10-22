using System;
using System.Collections.Generic;

namespace challenge_api_dotnet.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public string? Status { get; set; }

    public string? Tipo { get; set; }

    public int PatioIdPatio { get; set; }
    public virtual Patio PatioIdPatioNavigation { get; set; } = null!;
}
