namespace backend_notas.Models;

public class Empresa
{
    public int Id { get; set; }

    public string RazaoSocial { get; set; } = string.Empty;

    public string Cnpj { get; set; } = string.Empty;
}