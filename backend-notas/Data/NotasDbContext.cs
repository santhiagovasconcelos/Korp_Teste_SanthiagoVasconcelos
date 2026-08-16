using backend_notas.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_notas.Data;

public class NotasDbContext : DbContext
{
    public NotasDbContext(DbContextOptions<NotasDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotaFiscal> NotasFiscais { get; set; }
    public DbSet<ItemNota> ItensNota { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Empresa> Empresas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>()
            .HasOne(n => n.Cliente)
            .WithMany()
            .HasForeignKey(n => n.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotaFiscal>()
            .HasOne(n => n.Empresa)
            .WithMany()
            .HasForeignKey(n => n.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ItemNota>()
            .HasOne(i => i.NotaFiscal)
            .WithMany(n => n.Itens)
            .HasForeignKey(i => i.NotaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<NotaFiscal>()
            .HasIndex(n => n.Numero)
            .IsUnique();

        //seed de dados - Lembrar de avaliar inclusão de endpoint para adicionar Empresa e Cliente.
        modelBuilder.Entity<Empresa>().HasData(
            new Empresa
            {
                Id = -1,
                RazaoSocial = "Korp ERP Demo Ltda",
                Cnpj = "12345678000199"
            }
        );

        modelBuilder.Entity<Cliente>().HasData(
            new Cliente
            {
                Id = -1,
                Nome = "Cliente Teste 1",
                Documento = "11111111111"
            },
            new Cliente
            {
                Id = -2,
                Nome = "Cliente Teste 2",
                Documento = "22222222222"
            },
            new Cliente
            {
                Id = -3,
                Nome = "Cliente Teste 3",
                Documento = "33333333333"
            }
        );
    }
}