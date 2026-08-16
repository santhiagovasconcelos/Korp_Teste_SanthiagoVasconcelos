using backend_notas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_notas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresasController : ControllerBase
{
    private readonly NotasDbContext _context;

    public EmpresasController(NotasDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var empresas = await _context.Empresas
            .OrderBy(e => e.RazaoSocial)
            .ToListAsync();

        return Ok(empresas);
    }
}