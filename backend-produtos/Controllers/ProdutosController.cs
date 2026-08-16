using backend_produtos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend_produtos.Models;

namespace backend_produtos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{

    private readonly ProdutosDbContext _context;

    public ProdutosController(ProdutosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var produtos = await _context.Produtos.Where(p => p.Ativo).ToListAsync();

        return Ok(produtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        //Retornando apenas se o item estiver ativo
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

        if(produto == null)
        {
            return NotFound();
        }

        return Ok(produto);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Produto produto)
    {
        _context.Produtos.Add(produto);

        await _context.SaveChangesAsync();

        return CreatedAtAction( 
            nameof (GetById), 
            new { id = produto.Id }, 
            produto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Produto produto)
    {
        var produtoExistente = await _context.Produtos.FindAsync(id);

        if(produtoExistente == null)
        {
            return NotFound();
        }

        produtoExistente.Codigo = produto.Codigo;
        produtoExistente.Descricao = produto.Descricao;
        produtoExistente.Preco = produto.Preco;
        produtoExistente.Saldo = produto.Saldo;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if(produto == null)
        {
            return NotFound();
        }

        produto.Ativo = false;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
