using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RabbitaskWebAPI.RequestModels.Usuario
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [FromQuery]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [FromQuery]
        public string Senha { get; set; } = string.Empty;
    }
}
