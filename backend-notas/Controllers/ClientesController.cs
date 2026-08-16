using backend_notas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_notas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly NotasDbContext _context;

    public ClientesController(NotasDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var clientes = await _context.Clientes
            .OrderBy(c => c.Nome)
            .ToListAsync();

        return Ok(clientes);
    }
}