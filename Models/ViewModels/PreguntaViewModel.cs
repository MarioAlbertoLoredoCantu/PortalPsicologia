using System.Collections.Generic;
using PortalPsicologia.Models;

namespace PortalPsicologia.Models.ViewModels
{
    public class PreguntaViewModel
    {
        // Para el examen psicométrico
        public int Indice { get; set; } = 0;
        public Pregunta? PreguntaActual { get; set; }
        public int TotalPreguntas { get; set; } = 0;
        public int? RespuestaSeleccionada { get; set; }
public int TiempoRestante { get; set; }

        // Para la gestión de preguntas (admin)
        public Pregunta Pregunta { get; set; } = new Pregunta();
        public List<Pregunta> ListaPreguntas { get; set; } = new List<Pregunta>();
    }
}
