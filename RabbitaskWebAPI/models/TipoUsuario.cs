public class TipoUsuario
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public TipoUsuario()
    {
    }

    public TipoUsuario(int codigo, string nome)
    {
        Codigo = codigo;
        Nome = nome;
    }

}

