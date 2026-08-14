namespace backend_produtos.Models;

public class Produto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Saldo { get; set; }

}