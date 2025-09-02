public class Tag
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public Tag()
    {
    }

    public Tag(int codigo, string nome)
    {
        Codigo = codigo;
        Nome = nome;
    }
}
