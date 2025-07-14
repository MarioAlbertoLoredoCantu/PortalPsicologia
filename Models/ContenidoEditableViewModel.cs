namespace PortalPsicologia.Models.ViewModels
{
    public class ContenidoEditableViewModel
    {
        public int Id { get; set; }
        public string Clave { get; set; } = null!;
        public List<string> Lineas { get; set; } = new();
    }
}
