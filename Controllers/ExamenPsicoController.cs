using Microsoft.AspNetCore.Mvc;
using PortalPsicologia.Data;
using PortalPsicologia.Models;
using PortalPsicologia.Extensions; // Métodos de sesión personalizados
using Microsoft.AspNetCore.Http;
using System.Text.Json;

public class ExamenPsicoController : Controller
{
    private readonly PsicometricoContext _context;
    private const string SessionKeyPreguntas = "Preguntas";
    private const string SessionKeyRespuestas = "Respuestas";

    public ExamenPsicoController(PsicometricoContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Iniciar()
    {
        var listaPreguntas = _context.Preguntas.OrderBy(p => p.PreguntaId).ToList();

        if (listaPreguntas == null || listaPreguntas.Count == 0)
        {
            return Content("No hay preguntas disponibles para el examen.");
        }

        HttpContext.Session.Set<List<Pregunta>>(SessionKeyPreguntas, listaPreguntas);
        List<int?> respuestasUsuario = new List<int?>(new int?[listaPreguntas.Count]);
        HttpContext.Session.Set<List<int?>>(SessionKeyRespuestas, respuestasUsuario);

        return RedirectToAction(nameof(ExamenPsico), new { indice = 0 });
    }

    [HttpGet]
    public IActionResult ExamenPsico(int indice)
    {
        var listaPreguntas = HttpContext.Session.Get<List<Pregunta>>(SessionKeyPreguntas);
        var respuestasUsuario = HttpContext.Session.Get<List<int?>>(SessionKeyRespuestas);

        if (listaPreguntas == null || respuestasUsuario == null)
        {
            return RedirectToAction(nameof(Iniciar));
        }

        if (indice < 0 || indice >= listaPreguntas.Count)
        {
            return RedirectToAction(nameof(Iniciar));
        }

        var viewModel = new PreguntaViewModel
        {
            Indice = indice,
            PreguntaActual = listaPreguntas[indice],
            TotalPreguntas = listaPreguntas.Count,
            RespuestaSeleccionada = null
        };

        return View("examenPsico", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExamenPsico(PreguntaViewModel modelo)
    {
        var listaPreguntas = HttpContext.Session.Get<List<Pregunta>>(SessionKeyPreguntas);
        var respuestasUsuario = HttpContext.Session.Get<List<int?>>(SessionKeyRespuestas);

        if (listaPreguntas == null || respuestasUsuario == null)
        {
            return RedirectToAction(nameof(Iniciar));
        }

        int indiceActual = modelo.Indice;
        respuestasUsuario[indiceActual] = modelo.RespuestaSeleccionada;
        HttpContext.Session.Set<List<int?>>(SessionKeyRespuestas, respuestasUsuario);

        int siguiente = indiceActual + 1;
        if (siguiente < listaPreguntas.Count)
        {
            return RedirectToAction(nameof(ExamenPsico), new { indice = siguiente });
        }

        // ⚠️ Si tienes autenticación implementada, aquí puedes usar el ID del usuario autenticado
        var usuario = _context.Usuarios.FirstOrDefault(); // Por ahora usas el primero para pruebas

        if (usuario == null)
        {
            return Content("No se pudo identificar al usuario.");
        }

        for (int i = 0; i < listaPreguntas.Count; i++)
        {
            var respuestaEntidad = new RespuestaExamen
            {
                UsuarioId = usuario.UsuarioId,  // ✔️ Este es int, como en la base de datos
                PreguntaId = listaPreguntas[i].PreguntaId,
                Valor = respuestasUsuario[i] ?? 0
            };
            _context.RespuestasPsicometrico.Add(respuestaEntidad);
        }

    // Calcular el puntaje total
    int total = respuestasUsuario.Where(r => r.HasValue).Sum(r => r.Value);
    string diagnostico;

    if (total <= 21)
        diagnostico = "Ansiedad muy baja";
    else if (total <= 35)
        diagnostico = "Ansiedad moderada";
    else
        diagnostico = "Ansiedad severa";

    // Guardar resultado final
    var resultado = new ResultadoExamen
    {
        UsuarioId = usuario.UsuarioId,
        PuntajeTotal = total,
        Diagnostico = diagnostico
    };
    _context.ResultadosExamen.Add(resultado);

    _context.SaveChanges();

    // Pasar el resultado a la vista
    TempData["Puntaje"] = total;
    TempData["Diagnostico"] = diagnostico;

    HttpContext.Session.Remove(SessionKeyPreguntas);
    HttpContext.Session.Remove(SessionKeyRespuestas);

    return RedirectToAction(nameof(ExamenCompletado));
}

    [HttpGet]
    public IActionResult ExamenCompletado()
    {
        return View();
    }

    public IActionResult examenPsico()
    {
        return View();
    }
}

