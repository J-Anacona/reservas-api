namespace ReservasApi.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Documento { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "Pendiente";
    }
}