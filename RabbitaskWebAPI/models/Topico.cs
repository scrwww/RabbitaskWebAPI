public class Topico
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public Tarefa Tarefa { get; set; }
    public bool Concluido { get; set; }
    public Topico()
    {
    }

    public Topico(int codigo, string nome, Tarefa tarefa, bool concluido)
    {
        Codigo = codigo;
        Nome = nome;
        Tarefa = tarefa;
        Concluido = concluido;
    }
}

