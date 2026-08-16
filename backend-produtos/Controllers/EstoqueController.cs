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
    public async Task<IActionResult> BaixarEstoque(BaixaEstoqueRequest request)
    {
        //Validando quantidade
        if(request.Quantidade <= 0)
        {
            return BadRequest("A quantidade deve ser maior que zero.");
        }
        //Buscando o produto
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == request.ProdutoId && p.Ativo);

        if (produto == null)
        {
            return NotFound("Produto não encontrado.");
        }

        //validando o saldo
        if (produto.Saldo < request.Quantidade)
        {
            return BadRequest("Saldo insuficiente.");
        }

        //Verificando se já foi feito essa mesma baixa
        var movimentacaoExistente = await _context.MovimentacoesEstoque
            .AnyAsync(m =>
            m.ProdutoId == request.ProdutoId &&
            m.Tipo == "Saida" &&
            m.Referencia == request.Referencia);

        if (movimentacaoExistente)
        {
            return Conflict("Esta baixa de estoque já foi realizada.");
        }

        //Alterando saldo caso baixa ainda não já tenha sido realizada
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

    [HttpPost("estorno")]
    public async Task<IActionResult> EstornarEstoque(EstornoEstoqueRequest request)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == request.ProdutoId && p.Ativo);

        if (produto == null)
        {
            return NotFound("Produto não encontrado.");
        }

        var baixaOriginal = await _context.MovimentacoesEstoque
            .FirstOrDefaultAsync(m =>
                m.ProdutoId == request.ProdutoId &&
                m.Tipo == "Saida" &&
                m.Referencia == request.Referencia);

        if (baixaOriginal == null)
        {
            return BadRequest("Não existe baixa de estoque para esta referência.");
        }

        var estornoExistente = await _context.MovimentacoesEstoque
            .AnyAsync(m =>
                m.ProdutoId == request.ProdutoId &&
                m.Tipo == "Estorno" &&
                m.Referencia == request.Referencia);

        if (estornoExistente)
        {
            return Conflict("Esta movimentação já foi estornada.");
        }

        produto.Saldo += baixaOriginal.Quantidade;

        var movimentacao = new MovimentacaoEstoque
        {
            ProdutoId = produto.Id,
            Tipo = "Estorno",
            Quantidade = baixaOriginal.Quantidade,
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