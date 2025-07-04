using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PortalPsicologia.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
            [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; }

         [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
    [StringLength(100, ErrorMessage = "El correo no puede tener más de 100 caracteres")]
        [Remote(action: "VerificarCorreoUnico", controller: "Usuario", AdditionalFields = "UsuarioId", ErrorMessage = "Este correo ya está registrado")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La matrícula es obligatoria")]
    [RegularExpression(@"^[0-9]{1,5}$", ErrorMessage = "La matrícula debe contener solo números (máximo 5)")]
        [Remote(action: "VerificarMatriculaUnica", controller: "Usuario", AdditionalFields = "UsuarioId", ErrorMessage = "Esta matrícula ya está registrada")]
        public string Matricula { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; }
    }
}
