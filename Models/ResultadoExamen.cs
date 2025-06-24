namespace PortalPsicologia.Models;

public class ResultadoExamen
{
    public int ResultadoId { get; set; }
    public int UsuarioId { get; set; }
    public int PuntajeTotal { get; set; }
    public string Diagnostico { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
}