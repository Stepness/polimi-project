using Microsoft.AspNetCore.Mvc;

namespace PolimiProject.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("validate")]
    public IActionResult Validate()
    {
        return Ok();
    }
}