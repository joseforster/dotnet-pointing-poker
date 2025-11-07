using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


public class LoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    public string Erro { get; private set; }

    public LoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> OnPost([FromForm] string username, string password)
    {
        if (password != _configuration["Password"])
        {
            Erro = "Não é possível que tu não consiga copiar e colar um senha.";

            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
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