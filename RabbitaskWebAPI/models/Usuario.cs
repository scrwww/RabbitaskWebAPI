namespace RabbitaskWebAPI.models
{
    public class Usuario
    {
        public int CdUsuario { get; set; }
        public string NmUsuario { get; set; } = string.Empty;
        public string NmEmail { get; set; } = string.Empty;
        public string CdTelefone { get; set; } = string.Empty;
        public int CdTipoUsuario { get; set; }

        public TipoUsuario TipoUsuario { get; set; } = null!;

        public ICollection<ConexaoUsuario> Conexoes { get; set; } = new List<ConexaoUsuario>();
        public ICollection<ConexaoUsuario> Agentes { get; set; } = new List<ConexaoUsuario>();
    }
}