using Microsoft.AspNetCore.Mvc;

namespace backend_notas.Controllers;

[ApiController]
[Route("api/[controller]")]

public class NotasController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            mensagem = "API de notas funcionando"
        });
    }
}