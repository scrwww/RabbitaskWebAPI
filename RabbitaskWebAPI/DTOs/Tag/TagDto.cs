using System.ComponentModel.DataAnnotations;

namespace RabbitaskWebAPI.DTOs.Tag
{
    public class TagDto
    {
        public int Cd { get; set; }
        public string Nome { get; set; }
    }

    public class TagCreateDto
    {
        [Required(ErrorMessage = "O nome da tag é obrigatório")]
        [StringLength(250, ErrorMessage = "O nome deve ter no máximo 250 caracteres")]
        public string Nome { get; set; }
    }
}
