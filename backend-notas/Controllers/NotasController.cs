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

    //Carregar notas
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var notas = await _context.NotasFiscais
            .Include(n => n.Cliente)
            .Include(n => n.Empresa)
            .Include(n => n.Itens)
            .OrderByDescending(n => n.Numero)
            .ToListAsync();

        return Ok(notas.Select(n => new
        {
            n.Id,
            numero = n.Numero.ToString("D9"),
            n.DataEmissao,
            n.Status,

            cliente = new
            {
                n.Cliente.Id,
                n.Cliente.Nome
            },

            empresa = new
            {
                n.Empresa.Id,
                n.Empresa.RazaoSocial
            },

            quantidadeItens = n.Itens.Count,

            valorTotal = n.Itens.Sum(i =>
                i.Quantidade * i.ValorUnitario)
        }));
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

    //carregar nota por id
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

    //Adicionar item direto na nota com status aberta
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

    [HttpPost("{id}/processar")]
    public async Task<IActionResult> ProcessarNota(int id)
    {
        // Busca a nota e seus itens para realizar o processamento.
        var nota = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nota == null)
        {
            return NotFound("Nota fiscal não encontrada.");
        }

        // Somente notas abertas podem gerar movimentação de estoque.
        if (nota.Status != "Aberta")
        {
            return BadRequest("Somente notas abertas podem ser processadas.");
        }

        if (!nota.Itens.Any())
        {
            return BadRequest("A nota fiscal não possui itens.");
        }

        // Cria o cliente HTTP utilizado para comunicação com o backend-produtos.
        var client = _httpClientFactory.CreateClient("ProdutosApi");

        var referencia = $"Nota {nota.Numero:D9}";


        // Armazena somente os itens cuja baixa foi realizada com sucesso.
        // Essa lista será usada para compensação caso uma baixa posterior falhe.
        var itensBaixados = new List<ItemNota>();

        foreach (var item in nota.Itens)
        {
            // Monta a solicitação de baixa que será enviada ao backend-produtos.
            var request = new BaixaEstoqueRequest
            {
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade,
                Referencia = referencia
            };

            // Solicita ao microserviço de produtos a baixa do estoque deste item.
            var response = await client.PostAsJsonAsync(
                "/api/estoque/baixa",
                request);

            // Se alguma baixa falhar, desfaz as baixas realizadas anteriormente.
            // Isso evita deixar o estoque parcialmente movimentado enquanto
            // a nota permanece com status Aberta.
            if (!response.IsSuccessStatusCode)
            {
                foreach (var itemBaixado in itensBaixados)
                {
                    var estornoRequest = new
                    {
                        ProdutoId = itemBaixado.ProdutoId,
                        Referencia = referencia
                    };

                    await client.PostAsJsonAsync(
                        "/api/estoque/estorno",
                        estornoRequest);
                }

                return BadRequest(
                $"Não foi possível baixar o estoque do produto {item.CodigoProduto}. As baixas anteriores foram estornadas.");

            }
            //adicionando itens para confirmar a baixa
            itensBaixados.Add(item);
        }

        // Somente após todas as baixas terem sucesso a nota é considerada fechada.
        nota.Status = "Fechada";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            nota.Id,
            numero = nota.Numero.ToString("D9"),
            nota.Status
        });
    }



    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> CancelarNota(int id)
    {
        var nota = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nota == null)
        {
            return NotFound("Nota fiscal não encontrada.");
        }

        if (nota.Status == "Cancelada")
        {
            return BadRequest("A nota fiscal já está cancelada.");
        }

        // Nota ainda aberta: não houve movimentação de estoque. Pode ser feito o cancelamento direto
        if (nota.Status == "Aberta")
        {
            nota.Status = "Cancelada";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                nota.Id,
                numero = nota.Numero.ToString("D9"),
                nota.Status
            });
        }

        // Nota Fechada: os estoques precisam ser estornados antes do cancelamento.
        if (nota.Status == "Fechada")
        {
            var client = _httpClientFactory.CreateClient("ProdutosApi");

            var referencia = $"Nota {nota.Numero:D9}";

            foreach (var item in nota.Itens)
            {
                var estornoRequest = new
                {
                    ProdutoId = item.ProdutoId,
                    Referencia = referencia
                };

                var response = await client.PostAsJsonAsync(
                    "/api/estoque/estorno",
                    estornoRequest);

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(
                        $"Não foi possível estornar o estoque do produto {item.CodigoProduto}.");
                }
            }

            nota.Status = "Cancelada";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                nota.Id,
                numero = nota.Numero.ToString("D9"),
                nota.Status
            });
        }

        return BadRequest("Status da nota fiscal inválido.");
    }




}