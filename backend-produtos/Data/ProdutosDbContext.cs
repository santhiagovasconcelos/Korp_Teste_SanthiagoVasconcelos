using backend_produtos.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_produtos.Data;

public class ProdutosDbContext : DbContext
{
    public ProdutosDbContext(DbContextOptions<ProdutosDbContext> options) : base(options)
    {

    }

    public DbSet<Produto> Produtos { get; set; }
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MovimentacaoEstoque>()
            .HasOne(m => m.Produto)
            .WithMany(p => p.Movimentacoes)
            .HasForeignKey(m => m.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Dados iniciais para demonstração do sistema
        modelBuilder.Entity<Produto>().HasData(
            new Produto
            {
                Id = 1,
                Codigo = "P001",
                Descricao = "Notebook Industrial",
                Preco = 4500.00m,
                Saldo = 15,
                Ativo = true
            },
            new Produto
            {
                Id = 2,
                Codigo = "P002",
                Descricao = "Monitor 24\"",
                Preco = 900.00m,
                Saldo = 30,
                Ativo = true
            },
            new Produto
            {
                Id = 3,
                Codigo = "P003",
                Descricao = "Teclado Mecânico",
                Preco = 250.00m,
                Saldo = 50,
                Ativo = true
            },
            new Produto
            {
                Id = 4,
                Codigo = "P004",
                Descricao = "Mouse Wireless",
                Preco = 120.00m,
                Saldo = 80,
                Ativo = true
            },
            new Produto
            {
                Id = 5,
                Codigo = "P005",
                Descricao = "SSD NVMe 1TB",
                Preco = 480.00m,
                Saldo = 40,
                Ativo = true
            },
            new Produto
            {
                Id = 6,
                Codigo = "P006",
                Descricao = "Placa de Vídeo",
                Preco = 2800.00m,
                Saldo = 12,
                Ativo = true
            }
        );

    }
}