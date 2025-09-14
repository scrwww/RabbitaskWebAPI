using System.ComponentModel.DataAnnotations;

namespace RabbitaskWebAPI.RequestModels.Tarefa
{
    public class CriarTarefaRequest
    {
        [Required(ErrorMessage = "Nome da tarefa é obrigatório")]
        [StringLength(250, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 250 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres")]
        public string? Descricao { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Prioridade deve ser um valor positivo")]
        public int? CdPrioridade { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Formato de data inválido")]
        public DateTime? DataPrazo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID do usuário proprietário deve ser um valor positivo")]
        public int? CdUsuarioProprietario { get; set; }

        /// <summary>
        /// IDs das tags a serem associadas à tarefa
        /// </summary>
        public ICollection<int>? TagIds { get; set; }
    }
}
