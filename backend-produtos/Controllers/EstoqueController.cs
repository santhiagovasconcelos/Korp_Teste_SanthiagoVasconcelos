using backend_produtos.Data;
using backend_produtos.Dtos;
using backend_produtos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_produtos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstoqueController : ControllerBase
{
    private readonly ProdutosDbContext _context;

    public EstoqueController(ProdutosDbContext context)
    {
        _context = context;
    }

    [HttpGet("{produtoId}")]
    public async Task<IActionResult> GetSaldo(int produtoId)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.Ativo);

        if(produto == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            produtoId = produto.Id,
            codigo = produto.Codigo,
            descricao = produto.Descricao,
            saldo = produto.Saldo
        });
    }

    [HttpPost("baixa")]
    public async Task<IActionResult> BaixarEstoque(MovimentacaoEstoqueRequest request)
    {
        if(request.Quantidade <= 0)
        {
            return BadRequest("A quantidade deve ser maior que zero.");
        }

        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == request.ProdutoId && p.Ativo);

        if (produto == null)
        {
            return NotFound("Produto não encontrado.");
        }

        if (produto.Saldo < request.Quantidade)
        {
            return BadRequest("Saldo insuficiente.");
        }

        produto.Saldo -= request.Quantidade;

        var movimentacao = new MovimentacaoEstoque
        {
            ProdutoId = produto.Id,
            Tipo = "Saida",
            Quantidade = request.Quantidade,
            Data = DateTime.UtcNow,
            Referencia = request.Referencia
        };

        _context.MovimentacoesEstoque.Add(movimentacao);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            produtoId = produto.Id,
            saldoAtual = produto.Saldo
        });

    }
}