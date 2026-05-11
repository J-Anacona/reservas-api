using Microsoft.EntityFrameworkCore;
using ReservasApi.Data;

namespace ReservasApi.Services
{
    public class ActualizarEstadosService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ActualizarEstadosService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var citas = await context.Citas
                    .Where(c => c.Estado == "Pendiente"
                             && c.Fecha < DateTime.Now)
                    .ToListAsync();

                foreach (var cita in citas)
                {
                    cita.Estado = "Completada";
                }

                await context.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}