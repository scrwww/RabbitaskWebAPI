namespace RabbitaskWebAPI.DTOs.Usuario
{
    public class UsuarioCriadoDto
    {
        public int Cd { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TipoUsuario { get; set; }
    }
}
