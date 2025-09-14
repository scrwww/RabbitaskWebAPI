using RabbitaskWebAPI.DTOs.Prioridade;
using RabbitaskWebAPI.DTOs.Usuario;

namespace RabbitaskWebAPI.DTOs.Tarefa
{
    public class TarefaDetalheDto : TarefaResumoDto
    {
        public PrioridadeDto? Prioridade { get; set; }
        public UsuarioResumoDto? UsuarioProprietario { get; set; }
    }
}
