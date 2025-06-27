using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PortalPsicologia.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        [Required]
        public string Nombre { get; set; }

        [Required]
        [Remote(action: "VerificarCorreoUnico", controller: "Usuario", AdditionalFields = "UsuarioId", ErrorMessage = "Este correo ya está registrado")]
        public string Correo { get; set; }

        [Required]
        [Remote(action: "VerificarMatriculaUnica", controller: "Usuario", AdditionalFields = "UsuarioId", ErrorMessage = "Esta matrícula ya está registrada")]
        public string Matricula { get; set; }

        [Required]
        public string Rol { get; set; }
    }
}
