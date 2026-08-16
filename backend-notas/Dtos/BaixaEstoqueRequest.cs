namespace backend_notas.Dtos;

public class BaixaEstoqueRequest
{
    public int ProdutoId { get; set; }

    public int Quantidade { get; set; }

    public string Referencia { get; set; } = string.Empty;
}