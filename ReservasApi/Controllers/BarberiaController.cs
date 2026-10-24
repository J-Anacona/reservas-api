using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BarberiaController : ControllerBase
{
    private readonly IConfiguration _config;

    public BarberiaController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok(new
        {
            nombre = _config["Barberia:Nombre"],
            direccion = _config["Barberia:Direccion"],
            telefono = _config["Barberia:Telefono"]
        });
    }
}