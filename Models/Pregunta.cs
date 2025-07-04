using System.ComponentModel.DataAnnotations;

namespace PortalPsicologia.Models
{
    public class Pregunta
    {
        public int PreguntaId { get; set; }

        [Required(ErrorMessage = "El texto de la pregunta es obligatorio.")]
        [StringLength(400, ErrorMessage = "La pregunta no debe superar los 300 caracteres.")]
        public string Texto { get; set; }

        public bool Activa { get; set; } = true;

        public int Orden { get; set; } 
    }
}