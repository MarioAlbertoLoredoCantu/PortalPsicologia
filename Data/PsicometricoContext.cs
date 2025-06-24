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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pregunta>().ToTable("Pregunta"); // 👈 fuerza nombre real en SQL
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<RespuestaExamen>()
               .HasKey(r => r.RespuestaId);
            modelBuilder.Entity<ResultadoExamen>().HasKey(r => r.ResultadoId);
                modelBuilder.Entity<ResultadoExamen>().ToTable("ResultadoExamen");

        }


        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pregunta> Preguntas { get; set; }
        public DbSet<RespuestaExamen> RespuestasExamen { get; set; }
        public DbSet<RespuestaExamen> RespuestasPsicometrico { get; set; }
        public DbSet<ResultadoExamen> ResultadosExamen { get; set; }


    }
}
