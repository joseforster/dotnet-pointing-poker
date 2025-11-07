using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace PointingPoker.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public string Username { get; set; }

    public void OnGet()
    {
        this.Username = User.Identity.Name;
    }
}