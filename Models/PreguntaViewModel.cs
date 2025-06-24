namespace PortalPsicologia.Models;
public class PreguntaViewModel
{
    public int Indice { get; set; }            // Índice de la pregunta actual en la secuencia
    public Pregunta PreguntaActual { get; set; } = null!;
    public int? RespuestaSeleccionada { get; set; }        // Respuesta del usuario (0-3), null si no responde
    public int TotalPreguntas { get; set; }    // Cantidad total de preguntas (para mostrar progreso, opcional)
}