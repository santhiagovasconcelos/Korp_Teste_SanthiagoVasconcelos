namespace backend_produtos.Dtos;

public class EstornoEstoqueRequest
{
    public int ProdutoId { get; set; }
    public string Referencia { get; set; } = string.Empty;
}