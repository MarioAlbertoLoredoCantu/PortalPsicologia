using Microsoft.AspNetCore.Mvc;
using PortalPsicologia.Data;
using PortalPsicologia.Models;
using PortalPsicologia.Models.ViewModels;
using System.Linq;

public class ContenidoController : Controller
{
    private readonly PsicometricoContext _context;

    public ContenidoController(PsicometricoContext context)
    {
        _context = context;
    }

    // GET: Mostrar vista de edición según clave
    [HttpGet]
    public IActionResult Editar(string clave)
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");

        var contenido = _context.ContenidoEditable.FirstOrDefault(c => c.Clave == clave);
        if (contenido == null)
            return NotFound();

        if (clave == "temas")
        {
            var lineas = contenido.Texto.Split('<', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Replace("br>", "").Trim()).ToList();
            var viewModel = new ContenidoEditableViewModel
            {
                Id = contenido.Id,
                Clave = contenido.Clave,
                Lineas = lineas
            };
            return View("EditarTemas", viewModel);
        }
        else
        {
            return View(contenido); // vista Editar.cshtml
        }
    }

    // POST: Guardar cambios del formulario de contacto
    [HttpPost]
    public IActionResult Editar(ContenidoEditable model)
    {
        var contenido = _context.ContenidoEditable.FirstOrDefault(c => c.Id == model.Id);
        if (contenido == null)
            return NotFound();

        contenido.Texto = model.Texto ?? "";
        _context.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    // POST: Guardar cambios del formulario de temas
    [HttpPost]
    public IActionResult EditarTemas(ContenidoEditableViewModel model)
    {
        var contenido = _context.ContenidoEditable.FirstOrDefault(c => c.Id == model.Id);
        if (contenido == null)
            return NotFound();

        contenido.Texto = string.Join("<br>", model.Lineas.Where(l => !string.IsNullOrWhiteSpace(l)));
        _context.SaveChanges();

        return RedirectToAction("Index", "Home");
    }
}
