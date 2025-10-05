using System.ComponentModel.DataAnnotations;

namespace RabbitaskWebAPI.DTOs.Usuario
{
    /// <summary>
    /// DTO completo do usuário (para perfil próprio ou detalhes)
    /// </summary>
    public class UsuarioDto
    {
        public int Cd { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string? Telefone { get; set; }
        public string? TipoUsuario { get; set; }
    }

    /// <summary>
    /// DTO resumido do usuário (para referências em outras entidades)
    /// Não expõe informações sensíveis
    /// </summary>
    public class UsuarioResumoDto
    {
        public int Cd { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }
    }

    /// <summary>
    /// DTO para atualizar usuário
    /// </summary>
    public class UsuarioUpdateDto
    {
        [StringLength(64, ErrorMessage = "O nome deve ter no máximo 64 caracteres")]
        public string? Nome { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(254, ErrorMessage = "O email deve ter no máximo 254 caracteres")]
        public string? Email { get; set; }

        [StringLength(30, ErrorMessage = "O telefone deve ter no máximo 30 caracteres")]
        public string? Telefone { get; set; }

        [StringLength(255, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 255 caracteres")]
        public string? NovaSenha { get; set; }
    }

    /// <summary>
    /// DTO para criar usuário (usado no registro)
    /// </summary>
    public class UsuarioCreateDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(64, ErrorMessage = "O nome deve ter no máximo 64 caracteres")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(254, ErrorMessage = "O email deve ter no máximo 254 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 255 caracteres")]
        public string Senha { get; set; }

        [StringLength(30, ErrorMessage = "O telefone deve ter no máximo 30 caracteres")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "O tipo de usuário é obrigatório")]
        public int CdTipoUsuario { get; set; }
    }

    /// <summary>
    /// DTO para estatísticas do usuário
    /// </summary>
    public class UsuarioEstatisticasDto
    {
        public int TotalTarefas { get; set; }
        public int TarefasConcluidas { get; set; }
        public int TarefasPendentes { get; set; }
        public int TarefasAtrasadas { get; set; }
        public double TaxaConclusao { get; set; }
    }

    /// <summary>
    /// DTO para conectar Agente e Usuario Comum
    /// </summary>
    public class ConectarUsuariosDto
    {
        [Required(ErrorMessage = "O ID do agente é obrigatório")]
        public int CdAgente { get; set; }

        [Required(ErrorMessage = "O ID do usuário é obrigatório")]
        public int CdUsuario { get; set; }
    }
}