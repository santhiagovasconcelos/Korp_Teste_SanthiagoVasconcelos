using backend_produtos.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_produtos.Data;

public class ProdutosDbContext : DbContext
{
    public ProdutosDbContext(DbContextOptions<ProdutosDbContext> options) : base(options)
    {
        
    }

    public DbSet<Produto> Produtos { get; set; }
}