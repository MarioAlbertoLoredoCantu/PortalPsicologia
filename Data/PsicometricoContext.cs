using Microsoft.EntityFrameworkCore;
using PortalPsicologia.Models;

namespace PortalPsicologia.Data
{
    public class PsicometricoContext : DbContext
    {
        public PsicometricoContext(DbContextOptions<PsicometricoContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pregunta> Preguntas { get; set; }
        public DbSet<RespuestaPsicometrico> RespuestasExamen { get; set; }
        public DbSet<RespuestaPsicometrico> RespuestasPsicometrico { get; set; }

    }
}
