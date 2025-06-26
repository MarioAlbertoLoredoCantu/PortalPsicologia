using Microsoft.AspNetCore.Mvc;
using PortalPsicologia.Data;
using PortalPsicologia.Models;

public class UsuarioController : Controller
{
    private readonly PsicometricoContext _context;
    public UsuarioController(PsicometricoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult RegisterUser()
    {
        TempData["RolDetectado"] = HttpContext.Session.GetString("UsuarioRol");

        var rol = HttpContext.Session.GetString("UsuarioRol");
        if (rol != "Administrador" && rol != "Docente")
        {
            TempData["RolDetectado"] = rol ?? "NULO";
            return RedirectToAction("NoAutorizado", "Home");
        }

        return View();
    }

    [HttpPost]
    public IActionResult RegisterUser(Usuario usuario)
    {
        // Validación: matrícula existente
        if (_context.Usuarios.Any(u => u.Matricula == usuario.Matricula))
        {
            ModelState.AddModelError("Matricula", "Esta matrícula ya está registrada.");
        }

        // Validación: correo existente
        if (_context.Usuarios.Any(u => u.Correo == usuario.Correo))
        {
            ModelState.AddModelError("Correo", "Este correo ya está registrado.");
        }

        if (ModelState.IsValid)
        {
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "El usuario ha sido registrado exitosamente.";

            // Redireccionar al mismo formulario limpio
            return RedirectToAction(nameof(RegisterUser));
        }

        // Si hay errores, regresar con datos actuales
        return View(usuario);
    }

    public IActionResult ListaUsuarios()
    {
        TempData["RolDetectado"] = HttpContext.Session.GetString("UsuarioRol");

        var rol = HttpContext.Session.GetString("UsuarioRol");
        if (rol != "Administrador" && rol != "Docente")
        {
            TempData["RolDetectado"] = rol ?? "NULO";
            return RedirectToAction("NoAutorizado", "Home");
        }

        var usuarios = _context.Usuarios.ToList();
        return View(usuarios);
    }

    // AJAX: Validación en tiempo real (correo único)
    [AcceptVerbs("Get", "Post")]
    public IActionResult VerificarCorreoUnico(string correo)
    {
        bool existe = _context.Usuarios.Any(u => u.Correo == correo);
        return Json(!existe);
    }

    // AJAX: Validación en tiempo real (matrícula única)
    [AcceptVerbs("Get", "Post")]
    public IActionResult VerificarMatriculaUnica(string matricula)
    {
        bool existe = _context.Usuarios.Any(u => u.Matricula == matricula);
        return Json(!existe);
    }

    [HttpGet]
    public IActionResult ListaResultados()
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");
        if (rol != "Administrador" && rol != "Docente")
        {
            return RedirectToAction("NoAutorizado", "Home");
        }

        var resultados = (from r in _context.ResultadosExamen
                          join u in _context.Usuarios on r.UsuarioId equals u.UsuarioId
                          select new ResultadoDetalleViewModel
                          {
                              ResultadoId = r.ResultadoId,
                              Nombre = u.Nombre,
                              Correo = u.Correo,
                              Matricula = u.Matricula,
                              PuntajeTotal = r.PuntajeTotal,
                              Diagnostico = r.Diagnostico,
                              Fecha = r.Fecha
                          }).ToList();

        return View(resultados);
    }
}