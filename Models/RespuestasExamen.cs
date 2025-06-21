namespace PortalPsicologia.Models;

public class RespuestaExamen
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }  // FK al usuario
    public int PreguntaId { get; set; } // FK a Pregunta
    public int Valor { get; set; }      // 0-3 (en absoluto a severamente)
    public DateTime Fecha { get; set; }
}