using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using PortalPsicologia.Data;
using PortalPsicologia.Models;
using PortalPsicologia.Extensions;
using PortalPsicologia.Models.ViewModels;
using System.Linq;

public class ExamenPsicoController : Controller
{
    private readonly PsicometricoContext _context;
    private const string SessionKeyPreguntas = "Preguntas";
    private const string SessionKeyRespuestas = "Respuestas";

    private const string SessionKeyInicioExamen = "InicioExamen";
    private const int TiempoMaximoSegundos = 10; // 15 minutos

    public ExamenPsicoController(PsicometricoContext context)
    {
        _context = context;
    }

[HttpGet]
public IActionResult Iniciar()
{
    HttpContext.Session.Remove(SessionKeyPreguntas);
    HttpContext.Session.Remove(SessionKeyRespuestas);
    HttpContext.Session.Remove(SessionKeyInicioExamen);

    var listaPreguntas = _context.Preguntas
        .Where(p => p.Activa)
        .OrderBy(p => p.Orden)
        .ToList();

    if (listaPreguntas == null || listaPreguntas.Count == 0)
    {
        return Content("No hay preguntas activas disponibles para el examen.");
    }

    HttpContext.Session.Set(SessionKeyPreguntas, listaPreguntas);
    HttpContext.Session.Set(SessionKeyRespuestas, new List<int?>(new int?[listaPreguntas.Count]));
    HttpContext.Session.Set(SessionKeyInicioExamen, DateTime.Now);

    return RedirectToAction(nameof(ExamenPsico), new { indice = 0 });
}




    [HttpGet]
public IActionResult ExamenPsico(int indice)
{
    var inicioExamen = HttpContext.Session.Get<DateTime?>(SessionKeyInicioExamen);
    if (!inicioExamen.HasValue || (DateTime.Now - inicioExamen.Value).TotalSeconds >= TiempoMaximoSegundos)
    {
        return RedirectToAction(nameof(ExamenExpirado));
    }

    var listaPreguntas = HttpContext.Session.Get<List<Pregunta>>(SessionKeyPreguntas);
    var respuestasUsuario = HttpContext.Session.Get<List<int?>>(SessionKeyRespuestas);

    if (listaPreguntas == null || respuestasUsuario == null || indice < 0 || indice >= listaPreguntas.Count)
    {
        return RedirectToAction(nameof(Iniciar));
    }

    var tiempoRestante = TiempoMaximoSegundos - (int)(DateTime.Now - inicioExamen.Value).TotalSeconds;

    var viewModel = new PreguntaViewModel
    {
        Indice = indice,
        PreguntaActual = listaPreguntas[indice],
        TotalPreguntas = listaPreguntas.Count,
        RespuestaSeleccionada = respuestasUsuario[indice],
        TiempoRestante = tiempoRestante // NUEVO CAMPO en el ViewModel
    };

    return View("examenPsico", viewModel);
}
[HttpGet]
public IActionResult ExamenExpirado()
{
    HttpContext.Session.Remove("Preguntas");
    HttpContext.Session.Remove("Respuestas");
    HttpContext.Session.Remove("InicioExamen");
    return View("ExamenExpirado");
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
        HttpContext.Session.Set(SessionKeyRespuestas, respuestasUsuario);

        int siguiente = indiceActual + 1;
        if (siguiente < listaPreguntas.Count)
        {
            return RedirectToAction(nameof(ExamenPsico), new { indice = siguiente });
        }

        // ✅ Obtener el correo del usuario autenticado
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == email);

        if (usuario == null)
        {
            return Content("No se pudo identificar al usuario. Intente iniciar sesión nuevamente.");
        }

        // Guardar respuestas
        for (int i = 0; i < listaPreguntas.Count; i++)
        {
            var respuesta = new RespuestaExamen
            {
                UsuarioId = usuario.UsuarioId,
                PreguntaId = listaPreguntas[i].PreguntaId,
                Valor = respuestasUsuario[i] ?? 0,
                FechaRespuesta = DateTime.Now
            };
            _context.RespuestasPsicometrico.Add(respuesta);
        }

        // Calcular puntaje y diagnóstico
        int total = respuestasUsuario.Where(r => r.HasValue).Sum(r => r.Value);
        string diagnostico = total <= 21 ? "Ansiedad muy baja"
                             : total <= 35 ? "Ansiedad moderada"
                             : "Ansiedad severa";

        // Guardar resultado
        var resultado = new ResultadoExamen
        {
            UsuarioId = usuario.UsuarioId,
            PuntajeTotal = total,
            Diagnostico = diagnostico,
            Fecha = DateTime.Now
        };
        _context.ResultadosExamen.Add(resultado);
        _context.SaveChanges();

        // Mostrar resultado final
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
}
