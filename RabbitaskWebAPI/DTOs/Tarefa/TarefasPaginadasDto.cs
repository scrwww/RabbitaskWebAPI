namespace RabbitaskWebAPI.DTOs.Tarefa
{

    public class TarefasPaginadasDto
    {
        public IEnumerable<TarefaResumoDto> Tarefas { get; set; } = new List<TarefaResumoDto>();
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int ItensPorPagina { get; set; }
    }
}