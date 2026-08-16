namespace backend_notas.Models;

public class ItemNota
{
    public int Id { get; set; }

    public int NotaFiscalId { get; set; }

    public NotaFiscal NotaFiscal { get; set; } = null!;

    public int ProdutoId { get; set; }

    public string CodigoProduto { get; set; } = string.Empty;

    public string DescricaoProduto { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }

    public decimal ValorTotal => Quantidade * ValorUnitario;
}