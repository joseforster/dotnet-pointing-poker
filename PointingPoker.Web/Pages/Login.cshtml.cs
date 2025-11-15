using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PointingPoker.Enums;

public class LoginModel : PageModel
{
    public string Erro { get;private set; }
    
    public async Task<IActionResult> OnPost([FromForm] string username, string session, bool isCreatingSession)
    {
        if(isCreatingSession)
        {
            session = new Random().Next(1, 1000000).ToString();

            while (PointingPokerHub.DoesSessionExist(session))
            {
                session = new Random().Next(1, 1000000).ToString();
            }
        }
        else
        {
            if (string.IsNullOrEmpty(session))
            {
                Erro = $"Session is required.";
                return Page();
            }
            
            if (!PointingPokerHub.DoesSessionExist(session))
            {
                Erro = $"Session {session} does not exists.";
                return Page();
            }
        }

        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, username),
            new (nameof(EnumCustomClaimType.Session), session),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(3)),
            IssuedUtc =  DateTimeOffset.UtcNow,
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToPage("/Index");
    }
}