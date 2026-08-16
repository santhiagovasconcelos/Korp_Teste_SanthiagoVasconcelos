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
    }
}