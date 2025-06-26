using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
namespace PortalPsicologia.Models;

public class Usuario
{
public int UsuarioId { get; set; }

[Required(ErrorMessage = "El nombre es obligatorio")]
public string Nombre { get; set; }

[Required(ErrorMessage = "El correo es obligatorio")]
[EmailAddress(ErrorMessage = "Formato de correo inválido")]
[Remote(action: "VerificarCorreoUnico", controller: "Usuario", ErrorMessage = "Este correo ya está registrado")]
public string Correo { get; set; }

[Required(ErrorMessage = "La matrícula es obligatoria")]
[Remote(action: "VerificarMatriculaUnica", controller: "Usuario", ErrorMessage = "Esta matrícula ya existe")]
public string Matricula { get; set; }

[Required(ErrorMessage = "Selecciona un rol")]
    public string Rol { get; set; }
}