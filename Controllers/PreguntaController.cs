using Microsoft.AspNetCore.Mvc;
using PortalPsicologia.Data;
using PortalPsicologia.Models;
using PortalPsicologia.Models.ViewModels;

public class PreguntaController : Controller
{
    private readonly PsicometricoContext _context;

    public PreguntaController(PsicometricoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index(int? id)
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
        {
            return RedirectToAction("NoAutorizado", "Home");
        }

       var modelo = new PreguntaViewModel();
    
        if (id.HasValue)
        {
            var pregunta = _context.Preguntas.FirstOrDefault(p => p.PreguntaId == id.Value);
            if (pregunta != null)
            {
                modelo.Pregunta = pregunta;
            }
        }

        modelo.ListaPreguntas = _context.Preguntas.OrderBy(p => p.PreguntaId).ToList();

        return View(modelo);
    }

    public IActionResult Crear()
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");
        return View(); // Debe haber una vista llamada Crear.cshtml
    }

    [HttpPost]
    public IActionResult GuardarPregunta(PreguntaViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.ListaPreguntas = _context.Preguntas.OrderBy(p => p.PreguntaId).ToList();
            return View("Index", modelo);
        }

        if (modelo.Pregunta.PreguntaId == 0)
        {
            modelo.Pregunta.Activa = true;
            _context.Preguntas.Add(modelo.Pregunta);
            TempData["Mensaje"] = "Pregunta agregada correctamente.";
        }
        else
        {
            var existente = _context.Preguntas.Find(modelo.Pregunta.PreguntaId);
            if (existente != null)
            {
                existente.Texto = modelo.Pregunta.Texto;
                TempData["Mensaje"] = "Pregunta actualizada correctamente.";
            }
        }

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Editar(int id)
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");
        var pregunta = _context.Preguntas.FirstOrDefault(p => p.PreguntaId == id);
        if (pregunta == null) return NotFound();

        return View(pregunta); // Debe haber una vista llamada Editar.cshtml
    }

    [HttpGet]
    public IActionResult Desactivar(int id)
    {
        var pregunta = _context.Preguntas.FirstOrDefault(p => p.PreguntaId == id);
        if (pregunta == null)
        {
            return NotFound();
        }
        // Cambia el estado
        pregunta.Activa = false;
        _context.Update(pregunta);
        _context.SaveChanges();

        // Mensaje opcional
        TempData["Mensaje"] = "La pregunta fue desactivada correctamente.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Activar(int id)
    {
        var pregunta = _context.Preguntas.FirstOrDefault(p => p.PreguntaId == id);
        if (pregunta == null)
        {
            return NotFound();
        }

        pregunta.Activa = true;
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}
