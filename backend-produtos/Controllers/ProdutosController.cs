using Microsoft.AspNetCore.Mvc;

namespace backend_produtos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            mensagem = "API de produtos funcionando"
        });
    }
}
