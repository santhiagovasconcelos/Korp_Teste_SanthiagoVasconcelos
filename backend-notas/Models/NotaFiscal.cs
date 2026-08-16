namespace backend_notas.Models;

public class NotaFiscal
{
    public int Id { get; set; }

    public int Numero { get; set; }

    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public int EmpresaId { get; set; }

    public Empresa Empresa { get; set; } = null!;

    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Aberta";

    public List<ItemNota> Itens { get; set; } = new();
}