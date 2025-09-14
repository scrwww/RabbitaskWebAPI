using RabbitaskWebAPI.DTOs.Tag;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RabbitaskWebAPI.DTOs.Tarefa
{
    /// <summary>
    /// DTO para representar informações resumidas de uma tarefa
    /// Usado em listagens e operações que não requerem todos os detalhes
    /// </summary>
    public class TarefaResumoDto
    {
        /// <summary>
        /// Identificador único da tarefa
        /// </summary>
        public int Cd { get; set; }

        /// <summary>
        /// Nome/título da tarefa
        /// </summary>
        [Required]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada da tarefa (opcional)
        /// </summary>
        public string? Descricao { get; set; }

        /// <summary>
        /// Data limite para conclusão da tarefa
        /// </summary>
        [JsonPropertyName("data_prazo")]
        public DateTime? DataPrazo { get; set; }

        /// <summary>
        /// Data de criação da tarefa
        /// </summary>
        [JsonPropertyName("data_criacao")]
        public DateTime? DataCriacao { get; set; }

        /// <summary>
        /// Data de conclusão da tarefa (nula se ainda não concluída)
        /// </summary>
        [JsonPropertyName("data_conclusao")]
        public DateTime? DataConclusao { get; set; }

        /// <summary>
        /// Nome da prioridade da tarefa
        /// </summary>
        public string? Prioridade { get; set; }

        /// <summary>
        /// Lista de tags associadas à tarefa
        /// </summary>
        public List<TagResumoDto> Tags { get; set; } = new();

        /// <summary>
        /// Indica se a tarefa está concluída
        /// Propriedade calculada baseada em DataConclusao
        /// </summary>
        public bool Concluida => DataConclusao.HasValue;
    }
}