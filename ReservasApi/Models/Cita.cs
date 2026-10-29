namespace ReservasApi.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "Pendiente";

        public string UsuarioId { get; set; } = string.Empty;
        public Usuario? Usuario { get; set; }
    }
}