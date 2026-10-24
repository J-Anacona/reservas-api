namespace ReservasApi.Models
{
    public class CitaCrearDto
    {
        public DateTime Fecha { get; set; }
    }

    public class CitaActualizarDto
    {
        public DateTime Fecha { get; set; }
        public string? Estado { get; set; }
    }
}