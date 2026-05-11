using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasApi.Data;
using ReservasApi.Models;

[ApiController]
[Route("api/[controller]")]
public class CitasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CitasController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var citas = await _context.Citas
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                c.Apellido,
                c.Documento,
                c.Estado,
                Fecha = c.Fecha.ToString("dd/MM/yyyy HH:mm")
            })
            .ToListAsync();

        return Ok(citas);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Cita cita)
    {
        if (string.IsNullOrWhiteSpace(cita.Nombre))
            return BadRequest("Nombre requerido");

        if (string.IsNullOrWhiteSpace(cita.Apellido))
            return BadRequest("Apellido requerido");

        if (string.IsNullOrWhiteSpace(cita.Documento))
            return BadRequest("Documento requerido");

        if (!cita.Documento.All(char.IsDigit))
            return BadRequest("El documento solo debe contener números");

        if (cita.Fecha < DateTime.Now)
            return BadRequest("No puedes agendar citas en el pasado");

        if (await _context.Citas.AnyAsync(c =>
            c.Fecha.Year == cita.Fecha.Year &&
            c.Fecha.Month == cita.Fecha.Month &&
            c.Fecha.Day == cita.Fecha.Day &&
            c.Fecha.Hour == cita.Fecha.Hour &&
            c.Fecha.Minute == cita.Fecha.Minute))
        {
            return BadRequest("Horario ocupado");
        }

        cita.Estado = "Pendiente";

        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();

        return Ok(cita);
    }

    [HttpDelete("{documento}")]
    public async Task<IActionResult> Delete(string documento)
    {
        var cita = await _context.Citas
            .FirstOrDefaultAsync(c => c.Documento == documento);

        if (cita == null)
            return NotFound("Cita no encontrada");

        _context.Citas.Remove(cita);
        await _context.SaveChangesAsync();

        return Ok("Cita eliminada");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Cita cita)
    {
        if (id != cita.Id)
            return BadRequest("ID no coincide");

        bool existeHora = await _context.Citas
            .AnyAsync(c => c.Fecha == cita.Fecha && c.Id != id);

        if (existeHora)
            return BadRequest("Horario ocupado");

        _context.Entry(cita).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(cita);
    }
}