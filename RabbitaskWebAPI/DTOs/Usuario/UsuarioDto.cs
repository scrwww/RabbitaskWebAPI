using RabbitaskWebAPI.DTOs.TipoUsuario;
using RabbitaskWebAPI.Models;

namespace RabbitaskWebAPI.DTOs.Usuario
{
    public class UsuarioDto
    {
        public int Cd { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public TipoUsuarioDto? TipoUsuario { get; set; }
    }
}
