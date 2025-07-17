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

        modelo.ListaPreguntas = _context.Preguntas
    .OrderByDescending(p => p.Activa)
    .ThenBy(p => p.Orden)
    .ToList();


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

            // ✅ Código actualizado para definir correctamente el orden:
          int siguienteOrden = _context.Preguntas
        .Where(p => p.Activa)
        .Select(p => p.Orden)
        .ToList()
        .DefaultIfEmpty(0)
        .Max() + 1;

modelo.Pregunta.Orden = siguienteOrden;


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

    // 1. Desactiva la pregunta
    pregunta.Activa = false;
    _context.Update(pregunta);
    _context.SaveChanges();

    // 2. Reorganiza el orden de las preguntas activas
    var preguntasActivas = _context.Preguntas
        .Where(p => p.Activa)
        .OrderBy(p => p.Orden)
        .ToList();

    for (int i = 0; i < preguntasActivas.Count; i++)
    {
        preguntasActivas[i].Orden = i + 1;
    }

    _context.SaveChanges();

    TempData["Mensaje"] = "La pregunta fue desactivada y el orden actualizado correctamente.";
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

        // Asignar orden al activar
 int siguienteOrden = _context.Preguntas
    .Where(p => p.Activa)
    .Select(p => p.Orden)
    .ToList()
    .DefaultIfEmpty(0)
    .Max() + 1;
pregunta.Activa = true;
pregunta.Orden = siguienteOrden;


        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
public IActionResult Subir(int id)
{
    var pregunta = _context.Preguntas.FirstOrDefault(p => p.PreguntaId == id);
    if (pregunta == null || !pregunta.Activa) return NotFound();

    var anterior = _context.Preguntas
        .Where(p => p.Orden < pregunta.Orden && p.Activa)
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
    public IActionResult Bajar(int id)
    {
        var pregunta = _context.Preguntas.FirstOrDefault(p => p.PreguntaId == id);
        if (pregunta == null || !pregunta.Activa) return NotFound();

        var siguiente = _context.Preguntas
            .Where(p => p.Orden > pregunta.Orden && p.Activa)
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
[HttpGet]
public IActionResult InicializarOrden()
{
    var preguntasActivas = _context.Preguntas
        .Where(p => p.Activa)
        .OrderBy(p => p.PreguntaId) // O por texto si prefieres
        .ToList();

    int orden = 1;
    foreach (var p in preguntasActivas)
    {
        p.Orden = orden++;
    }

    _context.SaveChanges();

    TempData["Mensaje"] = "Orden asignado correctamente a preguntas activas.";
    return RedirectToAction("Index");
}
[HttpPost]
[IgnoreAntiforgeryToken] // ✅ Solo si no usas el token en el fetch
public IActionResult Reordenar([FromBody] List<int> nuevosIds)
{
    if (nuevosIds == null || nuevosIds.Count == 0)
    {
        return BadRequest(new { error = "La lista está vacía o no se recibió correctamente." });
    }

    try
    {
        var preguntas = _context.Preguntas
            .Where(p => nuevosIds.Contains(p.PreguntaId))
            .ToList();

        for (int i = 0; i < nuevosIds.Count; i++)
        {
            var pregunta = preguntas.FirstOrDefault(p => p.PreguntaId == nuevosIds[i]);
            if (pregunta != null)
            {
                pregunta.Orden = i + 1;
            }
        }

        _context.SaveChanges();

        return Json(new { mensaje = "Preguntas reordenadas con éxito." });
    }
    catch (Exception ex)
    {
        // 👇 devuelve el error en JSON en vez de vaciar la consola
        return StatusCode(500, new { error = ex.Message });
    }
}



}
