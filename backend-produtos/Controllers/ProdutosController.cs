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
        var produtos = await _context.Produtos.ToListAsync();

        return Ok(produtos);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Produto produto)
    {
        _context.Produtos.Add(produto);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = produto.Id }, produto);
    }
}
