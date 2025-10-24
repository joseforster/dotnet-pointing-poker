using Microsoft.AspNetCore.SignalR;

public class PointingPokerHub : Hub
{
    private static List<UserHubModel> _userHubModelList = new List<UserHubModel>();
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var username = this.Context.GetHttpContext().Request.Query["username"]; ;

        if (string.IsNullOrEmpty(username))
        {
            return;
        }

        await this.Clients.Caller.SendAsync("UserHubConnectedList", _userHubModelList);

        var userHubModel = new UserHubModel(this.Context.ConnectionId, username);

        if (!_userHubModelList.Contains(userHubModel))
        {
            await _semaphoreSlim.WaitAsync();

            if (!_userHubModelList.Contains(userHubModel))
            {
                _userHubModelList.Add(userHubModel);
            }

            _semaphoreSlim.Release();
        }

        await this.Clients.Others.SendAsync("UserHubConnected", userHubModel);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userHubModel = _userHubModelList.FirstOrDefault(f => f.ConnectionId == this.Context.ConnectionId);

        if (userHubModel != null)
        {
            _userHubModelList.Remove(userHubModel);

            await this.Clients.Others.SendAsync("UserHubDisconnected", userHubModel);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task OnCloseTheTab(string username)
    {
        var userHubModel = _userHubModelList.FirstOrDefault(f => f.Username == username);

        if (userHubModel != null)
        {
            _userHubModelList.Remove(userHubModel);

            await this.Clients.Others.SendAsync("UserHubDisconnected", userHubModel);
        }

        await base.OnDisconnectedAsync(new Exception());
    }
}