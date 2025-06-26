namespace PortalPsicologia.Models
{
    public class ResultadoDetalleViewModel
    {
        public int ResultadoId { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Matricula { get; set; }
        public int PuntajeTotal { get; set; }
        public string Diagnostico { get; set; }
        public DateTime Fecha { get; set; }
    }
}
