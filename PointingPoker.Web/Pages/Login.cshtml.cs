using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModel : PageModel
{
    public async Task<IActionResult> OnPost([FromForm] string username, string session)
    {
        if (string.IsNullOrEmpty(session))
        {
            session = new Random().Next(1, 1000000).ToString();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("Session", session),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = !Debugger.IsAttached,
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToPage("/Index");
    }
}