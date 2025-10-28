using System.ComponentModel.DataAnnotations;
using RabbitaskWebAPI.DTOs.Usuario;
using RabbitaskWebAPI.DTOs.Tag;
using RabbitaskWebAPI.DTOs.Prioridade;

namespace RabbitaskWebAPI.DTOs.Tarefa
{
    /// <summary>
    /// DTO para criar uma nova tarefa
    /// </summary>
    public class TarefaCreateDto
    {
        [Required(ErrorMessage = "O nome da tarefa é obrigatório")]
        [StringLength(250, ErrorMessage = "O nome da tarefa deve ter no máximo 250 caracteres")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O usuário é obrigatório")]
        public int CdUsuario { get; set; }

        [StringLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres")]
        public string? Descricao { get; set; }

        public int? CdPrioridade { get; set; }

        public DateTime? DataPrazo { get; set; }

        public List<string>? TagNomes { get; set; }
    }

    /// <summary>
    /// DTO para atualizar uma tarefa existente
    /// </summary>
    public class TarefaUpdateDto
    {
        [StringLength(250, ErrorMessage = "O nome da tarefa deve ter no máximo 250 caracteres")]
        public string? Nome { get; set; }

        [StringLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres")]
        public string? Descricao { get; set; }

        public int? CdPrioridade { get; set; }

        public DateTime? DataPrazo { get; set; }

        public List<string>? TagNomes { get; set; }
    }

    /// <summary>
    /// DTO principal de tarefa - usado para listagens e detalhes
    /// Inclui todas as informações, cliente decide o que usar
    /// </summary>
    public class TarefaDto
    {
        public int Cd { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public DateTime? DataPrazo { get; set; }
        public DateTime? DataCriacao { get; set; }
        public DateTime? DataConclusao { get; set; }

        public PrioridadeDto? Prioridade { get; set; }
        public UsuarioResumoDto Usuario { get; set; }
        public UsuarioResumoDto? UsuarioProprietario { get; set; }
        public List<TagDto>? Tags { get; set; } = new();
    }

    /// <summary>
    /// DTO retornado após criar uma tarefa
    /// </summary>
    public class TarefaCriadaDto
    {
        public int Cd { get; set; }
        public string Nome { get; set; }
        public DateTime? DataCriacao { get; set; }
    }
}