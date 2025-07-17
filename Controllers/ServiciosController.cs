using Microsoft.AspNetCore.Mvc;

namespace PortalPsicologia.Controllers
{
    public class ServiciosController : Controller
    {
        public IActionResult Alumnos()
        {
            ViewData["Title"] = "Atención a Alumnos";
            return View();
        }

        public IActionResult Estudiantes()
        {
            ViewData["Title"] = "Atención a Estudiantes";
            return View();
        }

        public IActionResult Docentes()
        {
            ViewData["Title"] = "Atención a Docentes";
            return View();
        }

        public IActionResult Administrativos()
        {
            ViewData["Title"] = "Atención personal administrativa";
            return View();
        }
    }
}
