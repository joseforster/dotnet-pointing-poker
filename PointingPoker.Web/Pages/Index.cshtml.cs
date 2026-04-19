using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PointingPoker.Enums;

namespace PointingPoker.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public string Username { get; private set; }
    
    public string SessionId { get; private set; }

    public void OnGet()
    {
        Username = User.Identity.Name;
        SessionId = User.FindFirstValue(nameof(EnumCustomClaimType.Session));
    }
    
    public async Task<IActionResult> OnPostExitSession()
    {
        await HttpContext.SignOutAsync();
        
        return RedirectToPage("/Login");
    }
}