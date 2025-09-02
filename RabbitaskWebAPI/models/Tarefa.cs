public class Tarefa
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public Prioridade Prioridade { get; set; }
    public string Descricao { get; set; }
    public DateTime DataPrazo { get; set; }
    public Usuario UsuarioProprietario { get; set; }
    public Usuario Usuario { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime DataConclusao { get; set; }
    public Tarefa()
    {
    }

    public Tarefa(int codigo, string nome, Prioridade prioridade, string descricao, DateTime dataPrazo, Usuario usuarioProprietario, Usuario usuario, DateTime dataCriacao, DateTime dataConclusao)
    {
        Codigo = codigo;
        Nome = nome;
        Prioridade = prioridade;
        Descricao = descricao;
        DataPrazo = dataPrazo;
        UsuarioProprietario = usuarioProprietario;
        Usuario = usuario;
        DataCriacao = dataCriacao;
        DataConclusao = dataConclusao;
    }
}

