namespace backend_notas.Dtos;

// DTO utilizado para representar os dados recebidos do backend-produtos
public class ProdutoResponse
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public int Saldo { get; set; }

    public bool Ativo { get; set; }
}