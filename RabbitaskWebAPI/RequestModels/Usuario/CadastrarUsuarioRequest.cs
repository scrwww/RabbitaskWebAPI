using System.ComponentModel.DataAnnotations;

namespace RabbitaskWebAPI.RequestModels.Usuario
{
    public class CadastrarUsuarioRequest
    {
        private const int MIN_PASSWORD_LENGTH = 8;

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(64, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 64 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [StringLength(254, ErrorMessage = "Email deve ter no máximo 254 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(255, MinimumLength = MIN_PASSWORD_LENGTH,
            ErrorMessage = "Senha deve ter entre 8 e 255 caracteres")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipo de usuário é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Tipo de usuário deve ser um valor positivo")]
        public int TipoUsuario { get; set; }

        [StringLength(30, ErrorMessage = "Telefone deve ter no máximo 30 caracteres")]
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        public string? Telefone { get; set; }
    }
}
