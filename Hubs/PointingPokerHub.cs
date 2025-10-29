using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;

public class PointingPokerHub : Hub
{
    private static List<UserHubModel> _userHubModelList = new List<UserHubModel>();
    private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    private static bool _areVotesBeingShowed = false;

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var username = this.Context.GetHttpContext().Request.Query["username"]; ;

        if (string.IsNullOrEmpty(username))
        {
            username = "Visitor";
        }

        await this.Clients.Caller.SendAsync("SetUserList", _userHubModelList);

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

        await this.Clients.Others.SendAsync("UserConnected", userHubModel);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await DisconnectUser(exception);
    }

    public async Task OnClosedTheTab()
    {
        await DisconnectUser();
    }

    public async Task OnUserVoted(string userVote)
    {
        Console.WriteLine("User voted --> " + userVote);

        var userHubModel = GetCurrentUserHubModel();

        if (!string.IsNullOrEmpty(userVote))
        {
            SetCurrentVote(userVote, userHubModel);
        }

        if (_areVotesBeingShowed)
        {
            await this.Clients.Others.SendAsync("UserHasVotedWithShowedVotes", userHubModel);
        }
        else
        {
            await this.Clients.Others.SendAsync("UserHasVoted", this.Context.ConnectionId);
        }
    }

    public async Task OnShowVotes()
    {
        await SetAreVotesBeingShowed(true);

        await this.Clients.All.SendAsync("ShowVotes", _userHubModelList);
    }

    public async Task OnClearVotes()
    {
        await SetAreVotesBeingShowed(false);

        await _semaphoreSlim.WaitAsync();

        foreach (var userHubModel in _userHubModelList)
        {
            userHubModel.CurrentVote = 0;
        }

        _semaphoreSlim.Release();

        await this.Clients.All.SendAsync("ClearVotes");
    }

    private UserHubModel GetCurrentUserHubModel()
    {
        return _userHubModelList.FirstOrDefault(f => f.ConnectionId == this.Context.ConnectionId);
    }

    private async Task SetAreVotesBeingShowed(bool isVotesBeingShowed)
    {
        await _semaphoreSlim.WaitAsync();

        _areVotesBeingShowed = isVotesBeingShowed;

        _semaphoreSlim.Release();
    }

    private static void SetCurrentVote(string userVote, UserHubModel userHubModel)
    {
        if (userVote == "?")
        {
            userHubModel.IsEmptyVote = true;
        }
        else
        {
            decimal parsedVote = 0M;

            decimal.TryParse(userVote, out parsedVote);

            userHubModel.CurrentVote = parsedVote;
        }
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

            await this.Clients.Others.SendAsync("UserDisconnected", userHubModel);
        }

        await base.OnDisconnectedAsync(exception ?? new Exception());
    }

}