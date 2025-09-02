namespace RabbitaskWebAPI.models
{
    public class ConexaoUsuario
    {
        public int CdUsuario { get; set; }
        public int CdUsuarioAgente { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Usuario UsuarioAgente { get; set; } = null!;
    }
}