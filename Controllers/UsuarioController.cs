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
        Console.WriteLine("¡Se ejecutó el POST!");

        if (_context.Usuarios.Any(u => u.Matricula == usuario.Matricula))
        {
            ModelState.AddModelError("Matricula", "Esta matrícula ya está registrada.");
        }

        if (_context.Usuarios.Any(u => u.Correo == usuario.Correo))
        {
            ModelState.AddModelError("Correo", "Este correo ya está registrado.");
        }

        if (ModelState.IsValid)
        {
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "El usuario ha sido registrado exitosamente.";
            return RedirectToAction(nameof(RegisterUser));
        }

        return View(usuario);
    }

    public IActionResult ListaUsuarios()
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");
        TempData["RolDetectado"] = rol;

        if (rol != "Administrador" && rol != "Docente")
        {
            return RedirectToAction("NoAutorizado", "Home");
        }

        var usuarios = _context.Usuarios.ToList();
        return View(usuarios);
    }

    // ✅ Validación remota única para correo
    [HttpGet]
    public IActionResult VerificarCorreoUnico(string correo, int UsuarioId = 0)
    {
        var existe = _context.Usuarios.Any(u => u.Correo == correo && u.UsuarioId != UsuarioId);
        return Json(!existe);
    }

    // ✅ Validación remota única para matrícula
    [HttpGet]
    public IActionResult VerificarMatriculaUnica(string matricula, int UsuarioId = 0)
    {
        var existe = _context.Usuarios.Any(u => u.Matricula == matricula && u.UsuarioId != UsuarioId);
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

        var usuarioExistente = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == usuario.UsuarioId);
        if (usuarioExistente == null)
        {
            TempData["ErrorMessage"] = "Usuario no encontrado.";
            return RedirectToAction("ListaUsuarios");
        }

        bool correoDuplicado = _context.Usuarios.Any(u => u.Correo == usuario.Correo && u.UsuarioId != usuario.UsuarioId);
        bool matriculaDuplicada = _context.Usuarios.Any(u => u.Matricula == usuario.Matricula && u.UsuarioId != usuario.UsuarioId);

        if (correoDuplicado)
            ModelState.AddModelError("Correo", "Este correo ya está registrado.");
        if (matriculaDuplicada)
            ModelState.AddModelError("Matricula", "Esta matrícula ya está registrada.");

        if (!ModelState.IsValid)
            return View(usuario);

        usuarioExistente.Nombre = usuario.Nombre;
        usuarioExistente.Correo = usuario.Correo;
        usuarioExistente.Matricula = usuario.Matricula;
        usuarioExistente.Rol = usuario.Rol;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
        return View(usuarioExistente);
    }
}
