using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReservasApi.Models;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Usuario> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<Usuario> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
    }
    [HttpPost("registro")]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("Nombre requerido");

        if (string.IsNullOrWhiteSpace(dto.Apellido))
            return BadRequest("Apellido requerido");

        if (string.IsNullOrWhiteSpace(dto.Telefono) ||
            !dto.Telefono.All(char.IsDigit))
            return BadRequest("El teléfono solo debe contener números");

        if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == dto.Telefono))
            return BadRequest("Ya existe un usuario con ese teléfono");

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return BadRequest("Ya existe un usuario con ese correo electrónico");

        var usuario = new Usuario
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            PhoneNumber = dto.Telefono
        };

        var resultado = await _userManager.CreateAsync(usuario, dto.Password);

        if (!resultado.Succeeded)
            return BadRequest(resultado.Errors.Select(e => e.Description));

        if (!await _roleManager.RoleExistsAsync("Usuario"))
            await _roleManager.CreateAsync(new IdentityRole("Usuario"));

        await _userManager.AddToRoleAsync(usuario, "Usuario");

        return Ok("Usuario registrado");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await _userManager.FindByEmailAsync(dto.Email);

        if (usuario == null)
            return NotFound("Este correo no está registrado. Crea una cuenta para continuar.");

        if (!await _userManager.CheckPasswordAsync(usuario, dto.Password))
            return Unauthorized("Revisa que tu correo y contraseña estén escritos correctamente.");

        var roles = await _userManager.GetRolesAsync(usuario);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id),
            new Claim(ClaimTypes.Email, usuario.Email!),
            new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}")
        };

        foreach (var rol in roles)
            claims.Add(new Claim(ClaimTypes.Role, rol));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credenciales);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            rol = roles.FirstOrDefault(),
            nombre = $"{usuario.Nombre} {usuario.Apellido}"
        });
    }
    [Authorize(Roles = "Barbero")]
    [HttpPost("asignar-barbero")]
    public async Task<IActionResult> AsignarBarbero(string email)
    {
        var usuario = await _userManager.FindByEmailAsync(email);

        if (usuario == null)
            return NotFound("Usuario no encontrado");

        if (await _userManager.IsInRoleAsync(usuario, "Barbero"))
            return BadRequest("Ya es barbero");

        await _userManager.AddToRoleAsync(usuario, "Barbero");

        return Ok("Rol de barbero asignado");
    }

    [Authorize(Roles = "Barbero")]
    [HttpPost("quitar-barbero")]
    public async Task<IActionResult> QuitarBarbero(string email)
    {
        var usuario = await _userManager.FindByEmailAsync(email);

        if (usuario == null)
            return NotFound("Usuario no encontrado");

        if (usuario.Email == User.FindFirstValue(ClaimTypes.Email))
            return BadRequest("No puedes quitarte el rol a ti mismo");

        await _userManager.RemoveFromRoleAsync(usuario, "Barbero");

        return Ok("Rol de barbero retirado");
    }
}