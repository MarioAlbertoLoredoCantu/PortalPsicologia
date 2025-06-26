using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PortalPsicologia.Data;
using PortalPsicologia.Models;

public class LoginController : Controller
{
    private readonly PsicometricoContext _context;

    public LoginController(PsicometricoContext context)
    {
        _context = context;
    }

   [HttpGet("login/google")]
public IActionResult LoginWithGoogle()
{
    return Challenge(new AuthenticationProperties
    {
        RedirectUri = Url.Action("GoogleCallback", "Login")
    }, GoogleDefaults.AuthenticationScheme);
}



    [HttpGet("login/google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return RedirectToAction("Index", "Home");

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

        // Buscar en la base de datos si ese correo está registrado
        var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == email);

        if (usuario == null)
        {
            return RedirectToAction("NoAutorizado", "Home");
        }

        // ✅ Guardar en sesión
        HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
        HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
        HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);

        // ✅ Crear claims e iniciar sesión con cookies
        var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, usuario.Nombre),
    new Claim(ClaimTypes.Email, usuario.Correo),
    new Claim(ClaimTypes.Role, usuario.Rol)
};

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // 🔄 Vuelve a guardar la sesión (puede haberse limpiado)
        HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
        HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
        HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);

        return RedirectToAction("Index", "Home");
    }
    
[HttpPost("login/logout")]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    HttpContext.Session.Clear();
    return RedirectToAction("Index", "Home");
}

}
