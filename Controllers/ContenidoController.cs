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

    // POST: Guardar cambios del formulario general
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

    // GET: Edición de servicio "servicio-alumnos"
    [HttpGet]
    public IActionResult EditarAlumnos()
    {
        var contenido = _context.ContenidoEditable.FirstOrDefault(c => c.Clave == "servicio-alumnos");
        if (contenido == null) return NotFound();
        return View("EditarServicio", contenido);
    }

    // POST: Guardar edición de servicio "servicio-alumnos"
    [HttpPost]
    public IActionResult EditarAlumnos(ContenidoEditable model)
    {
        var contenido = _context.ContenidoEditable.FirstOrDefault(c => c.Id == model.Id);
        if (contenido == null) return NotFound();

        contenido.Texto = model.Texto;
        _context.SaveChanges();

        return RedirectToAction("Alumnos", "Servicios");
    }

    // ✅ NUEVA ACCIÓN GET para editar cualquier servicio
    [HttpGet]
    public IActionResult EditarServicio(string clave)
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");

        var contenido = _context.ContenidoEditable.FirstOrDefault(c => c.Clave == clave);
        if (contenido == null)
            return NotFound();

        return View("EditarServicio", contenido);
    }

    // POST: Guardar edición de cualquier servicio
    [HttpPost]
    public IActionResult EditarServicio(ContenidoEditable model)
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");

        var contenido = _context.ContenidoEditable.FirstOrDefault(c => c.Id == model.Id);
        if (contenido == null)
            return NotFound();

        contenido.Texto = model.Texto;
        _context.SaveChanges();

        // Redirigir de nuevo a la vista de servicios según la clave
        return model.Clave switch
        {
            "servicio-alumnos" => RedirectToAction("Alumnos", "Servicios"),
            "servicio-docentes" => RedirectToAction("Docentes", "Servicios"),
            "servicio-estudiantes" => RedirectToAction("Estudiantes", "Servicios"),
            "servicio-administrativos" => RedirectToAction("Administrativos", "Servicios"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    [HttpGet]
    public IActionResult SubirPdf()
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SubirPdf(IFormFile pdfFile)
    {
        if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            return RedirectToAction("NoAutorizado", "Home");

        if (pdfFile != null && pdfFile.Length > 0 && Path.GetExtension(pdfFile.FileName).ToLower() == ".pdf")
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, Path.GetFileName(pdfFile.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            ViewBag.Mensaje = $"Archivo '{pdfFile.FileName}' subido exitosamente.";
        }
        else
        {
            ViewBag.Mensaje = "Solo se permiten archivos PDF.";
        }

        return View();
    }

public IActionResult VerPdf(string nombre)
{
    var nombreArchivo = nombre.Trim().ToLower()
        .Replace("á", "a").Replace("é", "e").Replace("í", "i")
        .Replace("ó", "o").Replace("ú", "u")
        .Replace("ñ", "n").Replace(" ", "_") + ".pdf";

    var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdf", nombreArchivo);

    if (!System.IO.File.Exists(ruta))
    {
        ViewBag.Tema = nombre;
        return View("PdfNoEncontrado");
    }

    var rutaRelativa = "/pdf/" + nombreArchivo;
    return Redirect(rutaRelativa);
}

[HttpGet]
public IActionResult SubirImagen()
{
    return View();
}

[HttpPost]
public async Task<IActionResult> SubirImagen(IFormFile imagenFile)
{
    if (imagenFile != null && imagenFile.Length > 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(imagenFile.FileName).ToLower()
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("ñ", "n").Replace(" ", "_") + Path.GetExtension(imagenFile.FileName);

        var rutaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "temas", fileName);

        using (var stream = new FileStream(rutaDestino, FileMode.Create))
        {
            await imagenFile.CopyToAsync(stream);
        }

        ViewBag.Mensaje = "Imagen subida correctamente.";
    }

    return View();
}

}
