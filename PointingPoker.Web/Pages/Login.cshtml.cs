using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PointingPoker.Enums;

public class LoginModel : PageModel
{
    public string Username { get; private set; } 
    public string Erro { get; private set; }

    private const int MIN_SESSION_NUMBER = 1000;
    private const int MAX_SESSION_NUMBER = 9999;
    private const string JWT_COOKIE_NAME = "CF_Authorization";
    
    private readonly string _unknowName = Guid.NewGuid().ToString().Substring(0, 8);
    
    public void OnGet()
    {
        var jwtCookie = HttpContext.Request.Cookies[JWT_COOKIE_NAME];

        if (string.IsNullOrEmpty(jwtCookie))
        {
            this.Username = _unknowName;
            return;
        }

        var jwtHandler = new JwtSecurityTokenHandler();

        var jwtToken = jwtHandler.ReadJwtToken(jwtCookie);

        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == EnumCustomClaimType.Email.ToString().ToLower())?.Value;

        if (string.IsNullOrEmpty(email))
        {
            this.Username = _unknowName;
            return;
        }
        
        this.Username = email.Split('@')[0];
    }

    public async Task<IActionResult> OnPostJoinSession([FromForm] string session, string username)
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

        return await SignInUser(session, username);
    }

    public async Task<IActionResult> OnPostCreateSession([FromForm] string session, string username)
    {
        session = new Random().Next(MIN_SESSION_NUMBER, MAX_SESSION_NUMBER).ToString();

        while (PointingPokerHub.DoesSessionExist(session))
        {
            session = new Random().Next(MIN_SESSION_NUMBER, MAX_SESSION_NUMBER).ToString();
        }

        return await SignInUser(session, username);
    }

    private async Task<IActionResult> SignInUser(string session, string username)
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
            IsPersistent = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToPage("/Index");
    }
}