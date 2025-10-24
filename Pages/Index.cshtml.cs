using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace PointingPoker.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public string Username { get; set; }

    private IHubContext<PointingPokerHub> _hubContext;

    public IndexModel(IHubContext<PointingPokerHub> hubContext)
    {
        this._hubContext = hubContext ?? throw new ArgumentNullException("hubContext");
    }

    public void OnGet()
    {
        this.Username = User.Identity.Name;
    }

    public void OnPost([FromForm] string vote)
    {

    }
}
