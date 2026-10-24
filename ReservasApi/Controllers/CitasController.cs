using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasApi.Data;
using ReservasApi.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CitasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CitasController(AppDbContext context)
    {
        _context = context;
    }

    private string UsuarioActualId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var query = _context.Citas.Include(c => c.Usuario).AsQueryable();

        if (!User.IsInRole("Barbero"))
            query = query.Where(c => c.UsuarioId == UsuarioActualId);

        var citas = await query
            .OrderBy(c => c.Fecha)
            .Select(c => new
            {
                c.Id,
                Nombre = c.Usuario!.Nombre,
                Apellido = c.Usuario.Apellido,
                Documento = c.Usuario.Documento,
                c.Estado,
                Fecha = c.Fecha.ToString("dd/MM/yyyy HH:mm")
            })
            .ToListAsync();

        return Ok(citas);
    }

    [HttpPost]
    public async Task<IActionResult> Post(CitaCrearDto dto)
    {
        if (dto.Fecha < DateTime.Now)
            return BadRequest("No puedes agendar citas en el pasado");

        if (await _context.Citas.AnyAsync(c =>
            c.Fecha.Year == dto.Fecha.Year &&
            c.Fecha.Month == dto.Fecha.Month &&
            c.Fecha.Day == dto.Fecha.Day &&
            c.Fecha.Hour == dto.Fecha.Hour &&
            c.Fecha.Minute == dto.Fecha.Minute))
        {
            return BadRequest("Horario ocupado");
        }

        var cita = new Cita
        {
            Fecha = dto.Fecha,
            Estado = "Pendiente",
            UsuarioId = UsuarioActualId
        };

        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();

        return Ok(new { cita.Id, cita.Fecha, cita.Estado });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cita = await _context.Citas.FirstOrDefaultAsync(c => c.Id == id);

        if (cita == null)
            return NotFound("Cita no encontrada");

        if (!User.IsInRole("Barbero") && cita.UsuarioId != UsuarioActualId)
            return Forbid();

        _context.Citas.Remove(cita);
        await _context.SaveChangesAsync();

        return Ok("Cita eliminada");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, CitaActualizarDto dto)
    {
        var cita = await _context.Citas.FirstOrDefaultAsync(c => c.Id == id);

        if (cita == null)
            return NotFound("Cita no encontrada");

        bool esBarbero = User.IsInRole("Barbero");

        if (!esBarbero && cita.UsuarioId != UsuarioActualId)
            return Forbid();

        bool existeHora = await _context.Citas
            .AnyAsync(c => c.Fecha == dto.Fecha && c.Id != id);

        if (existeHora)
            return BadRequest("Horario ocupado");

        cita.Fecha = dto.Fecha;

        if (esBarbero && !string.IsNullOrWhiteSpace(dto.Estado))
            cita.Estado = dto.Estado;

        await _context.SaveChangesAsync();

        return Ok(new { cita.Id, cita.Fecha, cita.Estado });
    }
}