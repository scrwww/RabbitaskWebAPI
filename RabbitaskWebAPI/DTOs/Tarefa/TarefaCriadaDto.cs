namespace RabbitaskWebAPI.DTOs.Tarefa
{
    public class TarefaCriadaDto
    {
        public int Cd { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime? DataCriacao { get; set; }
    }
}
