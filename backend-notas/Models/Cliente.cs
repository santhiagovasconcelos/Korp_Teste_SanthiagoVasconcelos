namespace backend_notas.Models;

public class Cliente
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Documento { get; set; } = string.Empty;
}