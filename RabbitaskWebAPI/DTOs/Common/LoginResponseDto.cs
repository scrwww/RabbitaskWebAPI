using RabbitaskWebAPI.DTOs.Usuario;

namespace RabbitaskWebAPI.DTOs.Common
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioDto Usuario { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
