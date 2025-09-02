public class Anexo
{
    public int Codigo { get; set; }
    public Tarefa Tarefa { get; set; }
    public string NomeCaminhoAnexo { get; set; }

    public Anexo()
    {
    }

    public Anexo(int codigo, Tarefa tarefa, string nomeCaminhoAnexo)
    {
        Codigo = codigo;
        Tarefa = tarefa;
        NomeCaminhoAnexo = nomeCaminhoAnexo;
    }
}

