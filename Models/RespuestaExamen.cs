namespace PortalPsicologia.Models;

public class RespuestaExamen
{
     public int RespuestaId { get; set; }
    public int UsuarioId { get; set; }
  // Id del usuario (string si usas Identity)
    public int PreguntaId { get; set; }
    public int? Valor { get; set; }  // 0-3 o null si no respondió
    public DateTime FechaRespuesta { get; set; } = DateTime.Now;

}