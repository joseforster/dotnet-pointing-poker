using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PointingPoker.Enums;

public class LoginModel : PageModel
{
    public string Erro { get; private set; }

    private const int MIN_SESSION_NUMBER = 1;
    private const int MAX_SESSION_NUMBER = 1000000;

    public async Task<IActionResult> OnPostJoinSession([FromForm] string username, string session)
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
        
        return await SignInUser(username, session);
    }

    public async Task<IActionResult> OnPostCreateSession([FromForm] string username, string session)
    {
        session = new Random().Next(MIN_SESSION_NUMBER, MAX_SESSION_NUMBER).ToString();

        while (PointingPokerHub.DoesSessionExist(session))
        {
            session = new Random().Next(MIN_SESSION_NUMBER, MAX_SESSION_NUMBER).ToString();
        }

        return await SignInUser(username, session);
    }

    private async Task<IActionResult> SignInUser(string username, string session)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(nameof(EnumCustomClaimType.Session), session),
            new(nameof(EnumCustomClaimType.Guid), Guid.NewGuid().ToString()),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(3)),
            IssuedUtc = DateTimeOffset.UtcNow,
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToPage("/Index");
    }
}