using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;

public class PointingPokerHub : Hub
{
    private static List<UserHubModel> _userHubModelList = new List<UserHubModel>();
    private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    private static bool _isVotesBeingShowed = false;

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var username = this.Context.GetHttpContext().Request.Query["username"]; ;

        if (string.IsNullOrEmpty(username))
        {
            username = "Visitor";
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
        await DisconnectUser(exception);
    }

    public async Task OnClosedTheTab()
    {
        await DisconnectUser();
    }

    private async Task DisconnectUser(Exception? exception = null)
    {
        var userHubModel = GetCurrentUserHubModel();

        if (userHubModel != null)
        {
            await _semaphoreSlim.WaitAsync();

            userHubModel = GetCurrentUserHubModel();

            if (userHubModel != null)
            {
                _userHubModelList.Remove(userHubModel);
            }

            _semaphoreSlim.Release();

            await this.Clients.Others.SendAsync("UserHubDisconnected", userHubModel);
        }

        await base.OnDisconnectedAsync(exception ?? new Exception());
    }

    public async Task OnUserVoted(string userVote)
    {
        Console.WriteLine("User voted --> " + userVote);

        var userHubModel = GetCurrentUserHubModel();

        decimal parsedVote;

        decimal.TryParse(userVote, out parsedVote);

        userHubModel.CurrentVote = parsedVote;

        if (_isVotesBeingShowed)
        {

        }
        else
        {
            await this.Clients.Others.SendAsync("UserHasVoted", this.Context.ConnectionId);
        }

    }

    public async Task OnShowVotes()
    {
        await SetIsVotesBeingShowed(true);

        await this.Clients.All.SendAsync("ShowVotes", _userHubModelList);
    }

    private UserHubModel GetCurrentUserHubModel()
    {
        return _userHubModelList.FirstOrDefault(f => f.ConnectionId == this.Context.ConnectionId);
    }

    private async Task SetIsVotesBeingShowed(bool isVotesBeingShowed)
    {
        await _semaphoreSlim.WaitAsync();

        _isVotesBeingShowed = isVotesBeingShowed;

        _semaphoreSlim.Release();
    }
}