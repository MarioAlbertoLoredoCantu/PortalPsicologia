using System.ComponentModel.DataAnnotations;

namespace PortalPsicologia.Models
{
    public class Pregunta
    {
        public int PreguntaId { get; set; }

        [Required(ErrorMessage = "El texto de la pregunta es obligatorio.")]
        public string Texto { get; set; }

        public bool Activa { get; set; } = true;
    }
}