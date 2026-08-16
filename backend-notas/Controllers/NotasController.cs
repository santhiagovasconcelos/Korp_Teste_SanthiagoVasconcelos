using backend_notas.Data;
using backend_notas.Dtos;
using backend_notas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace backend_notas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotasController : ControllerBase
{
    private readonly NotasDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public NotasController(NotasDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> Post(CriarNotaRequest request)
    {
        var clienteExiste = await _context.Clientes
            .AnyAsync(c => c.Id == request.ClienteId);

        if (!clienteExiste)
        {
            return BadRequest("Cliente não encontrado.");
        }

        var empresaExiste = await _context.Empresas
            .AnyAsync(e => e.Id == request.EmpresaId);

        if (!empresaExiste)
        {
            return BadRequest("Empresa não encontrada.");
        }

        var ultimoNumero = await _context.NotasFiscais
            .MaxAsync(n => (int?)n.Numero) ?? 0;

        var nota = new NotaFiscal
        {
            Numero = ultimoNumero + 1,
            ClienteId = request.ClienteId,
            EmpresaId = request.EmpresaId,
            DataEmissao = DateTime.UtcNow,
            Status = "Aberta"
        };

        _context.NotasFiscais.Add(nota);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = nota.Id },
            new
            {
                nota.Id,
                numero = nota.Numero.ToString("D9"),
                nota.ClienteId,
                nota.EmpresaId,
                nota.DataEmissao,
                nota.Status
            });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var nota = await _context.NotasFiscais
            .Include(n => n.Cliente)
            .Include(n => n.Empresa)
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nota == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            nota.Id,
            numero = nota.Numero.ToString("D9"),
            nota.DataEmissao,
            nota.Status,

            cliente = new
            {
                nota.Cliente.Id,
                nota.Cliente.Nome,
                nota.Cliente.Documento
            },

            empresa = new
            {
                nota.Empresa.Id,
                nota.Empresa.RazaoSocial,
                nota.Empresa.Cnpj
            },

            itens = nota.Itens.Select(i => new
            {
                i.Id,
                i.ProdutoId,
                i.CodigoProduto,
                i.DescricaoProduto,
                i.Quantidade,
                i.ValorUnitario,
                i.ValorTotal
            })
        });
    }


    [HttpPost("{id}/itens")]
    public async Task<IActionResult> AdicionarItem(int id, AdicionarItemRequest request)
    {
        if (request.Quantidade <= 0)
        {
            return BadRequest("A quantidade deve ser maior que zero.");
        }

        var nota = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nota == null)
        {
            return NotFound("Nota fiscal não encontrada.");
        }

        if (nota.Status != "Aberta")
        {
            return BadRequest("Só é possível adicionar itens em notas abertas.");
        }

        var itemExistente = nota.Itens
            .Any(i => i.ProdutoId == request.ProdutoId);

        if (itemExistente)
        {
            return Conflict("Este produto já foi adicionado à nota.");
        }

        var client = _httpClientFactory.CreateClient("ProdutosApi");

        var response = await client.GetAsync(
            $"/api/produtos/{request.ProdutoId}");

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest("Produto não encontrado ou inativo.");
        }

        var produto = await response.Content
            .ReadFromJsonAsync<ProdutoResponse>();

        if (produto == null)
        {
            return BadRequest("Não foi possível obter os dados do produto.");
        }

        if (produto.Saldo < request.Quantidade)
        {
            return BadRequest("Saldo insuficiente para este produto.");
        }

        var item = new ItemNota
        {
            NotaFiscalId = nota.Id,
            ProdutoId = produto.Id,
            CodigoProduto = produto.Codigo,
            DescricaoProduto = produto.Descricao,
            Quantidade = request.Quantidade,
            ValorUnitario = produto.Preco
        };

        _context.ItensNota.Add(item);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            item.Id,
            item.ProdutoId,
            item.CodigoProduto,
            item.DescricaoProduto,
            item.Quantidade,
            item.ValorUnitario,
            item.ValorTotal
        });
    }

    [HttpPut("{notaId}/itens/{itemId}")]
    public async Task<IActionResult> AtualizarItem(int notaId, int itemId, AtualizarItemRequest request)
    {
        if (request.Quantidade <= 0)
        {
            return BadRequest("A quantidade deve ser maior que zero.");
        }

        var nota = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == notaId);

        if (nota == null)
        {
            return NotFound("Nota fiscal não encontrada.");
        }

        if (nota.Status != "Aberta")
        {
            return BadRequest("Só é possível editar itens de notas abertas.");
        }

        var item = nota.Itens
            .FirstOrDefault(i => i.Id == itemId);

        if (item == null)
        {
            return NotFound("Item não encontrado nesta nota.");
        }

        var client = _httpClientFactory.CreateClient("ProdutosApi");

        var response = await client.GetAsync(
            $"/api/produtos/{item.ProdutoId}");

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest("Produto não encontrado ou inativo.");
        }

        var produto = await response.Content
            .ReadFromJsonAsync<ProdutoResponse>();

        if (produto == null)
        {
            return BadRequest("Não foi possível obter os dados do produto.");
        }

        if (produto.Saldo < request.Quantidade)
        {
            return BadRequest("Saldo insuficiente para a quantidade informada.");
        }

        item.Quantidade = request.Quantidade;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            item.Id,
            item.ProdutoId,
            item.CodigoProduto,
            item.DescricaoProduto,
            item.Quantidade,
            item.ValorUnitario,
            item.ValorTotal
        });
    }

    [HttpDelete("{notaId}/itens/{itemId}")]
    public async Task<IActionResult> RemoverItem(int notaId, int itemId)
    {
        var nota = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == notaId);

        if (nota == null)
        {
            return NotFound("Nota fiscal não encontrada.");
        }

        if (nota.Status != "Aberta")
        {
            return BadRequest("Só é possível remover itens de notas abertas.");
        }

        var item = nota.Itens
            .FirstOrDefault(i => i.Id == itemId);

        if (item == null)
        {
            return NotFound("Item não encontrado nesta nota.");
        }

        _context.ItensNota.Remove(item);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}