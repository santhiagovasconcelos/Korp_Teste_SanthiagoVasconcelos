namespace backend_produtos.Models;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public string Tipo { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public string Referencia { get; set; } = string.Empty; //Número da nota... Ex: 00000018
}