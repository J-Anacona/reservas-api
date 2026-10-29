using Microsoft.AspNetCore.Identity;

namespace ReservasApi.Models
{
    public class Usuario : IdentityUser
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;

        public List<Cita> Citas { get; set; } = new();
    }
}