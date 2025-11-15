using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PointingPoker.Enums;

namespace PointingPoker.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public string Username { get; private set; }
    
    public string SessionId { get; private set; }

    public void OnGet()
    {
        this.Username = User.Identity.Name;
        this.SessionId = User.FindFirstValue(nameof(EnumCustomClaimType.Session));
    }
}