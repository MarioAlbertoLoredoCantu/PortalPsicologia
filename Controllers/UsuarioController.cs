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

    public IActionResult ListaUsuariosParaEliminar()
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");
        if (rol != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");
        var usuarios = _context.Usuarios.ToList();
        return View(usuarios);
    }

    [HttpPost]
    public IActionResult EliminarUsuario(int id)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");
        if (rol != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");
        var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
        }
        return RedirectToAction("ListaUsuariosParaEliminar");
    }

    [HttpGet]
    public IActionResult EditarUsuario(int id)
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
        {
            return RedirectToAction("NoAutorizado", "Home");
        }
        var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == id);
        if (usuario == null)
        {
            TempData["ErrorMessage"] = "Usuario no encontrado.";
            return RedirectToAction("ListaUsuarios");
        }

        return View(usuario);
    }

    [HttpPost]
    public IActionResult EditarUsuario(Usuario usuario)
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");
        // Verifica si el usuario existe
        var usuarioExistente = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == usuario.UsuarioId);
        if (usuarioExistente == null)
        {
            TempData["ErrorMessage"] = "Usuario no encontrado.";
            return RedirectToAction("ListaUsuarios");
        }

        // Validar duplicados (excepto el mismo ID)
        bool correoDuplicado = _context.Usuarios.Any(u => u.Correo == usuario.Correo && u.UsuarioId != usuario.UsuarioId);
        bool matriculaDuplicada = _context.Usuarios.Any(u => u.Matricula == usuario.Matricula && u.UsuarioId != usuario.UsuarioId);

        if (correoDuplicado)
            ModelState.AddModelError("Correo", "Este correo ya está registrado.");
        if (matriculaDuplicada)
            ModelState.AddModelError("Matricula", "Esta matrícula ya está registrada.");

        if (!ModelState.IsValid)
            return View(usuario);

        // Actualiza campos
        usuarioExistente.Nombre = usuario.Nombre;
        usuarioExistente.Correo = usuario.Correo;
        usuarioExistente.Matricula = usuario.Matricula;
        usuarioExistente.Rol = usuario.Rol;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
        return View(usuarioExistente); // Regresa a la misma vista con datos actualizados
    }

    [HttpGet]
    public IActionResult VerificarMatriculaUnica(string matricula, int UsuarioId)
    {
        var existe = _context.Usuarios.Any(u => u.Matricula == matricula && u.UsuarioId != UsuarioId);
        return Json(!existe);
    }

    [HttpGet]
    public IActionResult VerificarCorreoUnico(string correo, int UsuarioId)
    {
        var existe = _context.Usuarios.Any(u => u.Correo == correo && u.UsuarioId != UsuarioId);
        return Json(!existe);
    }
}