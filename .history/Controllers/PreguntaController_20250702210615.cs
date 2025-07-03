using Microsoft.AspNetCore.Mvc;
using PortalPsicologia.Data;
using PortalPsicologia.Models;
using PortalPsicologia.Models.ViewModels;
using System.Linq;

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

        modelo.ListaPreguntas = _context.Preguntas.OrderBy(p => p.Orden).ToList();

        return View(modelo);
    }

    public IActionResult Crear()
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");
        return View();
    }

    [HttpPost]
    public IActionResult GuardarPregunta(PreguntaViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.ListaPreguntas = _context.Preguntas.OrderBy(p => p.Orden).ToList();
            return View("Index", modelo);
        }

        if (modelo.Pregunta.PreguntaId == 0)
        {
            modelo.Pregunta.Activa = true;
            modelo.Pregunta.Orden = _context.Preguntas.Any() ? _context.Preguntas.Max(p => p.Orden) + 1 : 1;
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

        return View(pregunta);
    }

    [HttpGet]
    public IActionResult Desactivar(int id)
    {
        var pregunta = _context.Preguntas.FirstOrDefault(p => p.PreguntaId == id);
        if (pregunta == null)
        {
            return NotFound();
        }

        pregunta.Activa = false;
        _context.Update(pregunta);
        _context.SaveChanges();

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

    [HttpPost]
    public IActionResult SubirOrden(int id)
    {
        var pregunta = _context.Preguntas.FirstOrDefault(p => p.PreguntaId == id);
        if (pregunta == null) return NotFound();

        var anterior = _context.Preguntas
            .Where(p => p.Activa && p.Orden < pregunta.Orden)
            .OrderByDescending(p => p.Orden)
            .FirstOrDefault();

        if (anterior != null)
        {
            int temp = pregunta.Orden;
            pregunta.Orden = anterior.Orden;
            anterior.Orden = temp;

            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult BajarOrden(int id)
    {
        var pregunta = _context.Preguntas.FirstOrDefault(p => p.PreguntaId == id);
        if (pregunta == null) return NotFound();

        var siguiente = _context.Preguntas
            .Where(p => p.Activa && p.Orden > pregunta.Orden)
            .OrderBy(p => p.Orden)
            .FirstOrDefault();

        if (siguiente != null)
        {
            int temp = pregunta.Orden;
            pregunta.Orden = siguiente.Orden;
            siguiente.Orden = temp;

            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}
