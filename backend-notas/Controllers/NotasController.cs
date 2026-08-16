using backend_notas.Data;
using backend_notas.Dtos;
using backend_notas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_notas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotasController : ControllerBase
{
    private readonly NotasDbContext _context;

    public NotasController(NotasDbContext context)
    {
        _context = context;
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
}