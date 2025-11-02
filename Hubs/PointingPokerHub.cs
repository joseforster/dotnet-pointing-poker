using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;

public class PointingPokerHub : Hub
{
    private static List<UserModel> _userModelList = new List<UserModel>();
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

        await this.Clients.Caller.SendAsync("SetUserList", _userModelList, _areVotesBeingShowed);

        var userHubModel = new UserModel(this.Context.ConnectionId, username);

        if (!_userModelList.Contains(userHubModel))
        {
            await _semaphoreSlim.WaitAsync();

            if (!_userModelList.Contains(userHubModel))
            {
                _userModelList.Add(userHubModel);
            }

            _semaphoreSlim.Release();
        }

        await this.Clients.Others.SendAsync("UserConnected", userHubModel);

        if (_areVotesBeingShowed)
        {
            await this.Clients.Caller.SendAsync("SetVoteResult", GetVoteModel());
        }
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

        var userModel = GetCurrentUserModel();

        userModel.CurrentVote = userVote;

        if (_areVotesBeingShowed)
        {
            await this.Clients.Others.SendAsync("UserHasVotedWithShowedVotes", userModel);
            await this.Clients.All.SendAsync("SetVoteResult", GetVoteModel());
        }
        else
        {
            await this.Clients.Others.SendAsync("UserHasVoted", this.Context.ConnectionId);
        }
    }

    public async Task OnShowVotes()
    {
        await SetAreVotesBeingShowed(true);

        await this.Clients.All.SendAsync("ShowVotes", _userModelList);
        await this.Clients.All.SendAsync("SetVoteResult", GetVoteModel());
    }

    public async Task OnClearVotes()
    {
        await SetAreVotesBeingShowed(false);

        await _semaphoreSlim.WaitAsync();

        _userModelList.ForEach(fe => fe.CurrentVote = string.Empty);

        _semaphoreSlim.Release();

        await this.Clients.All.SendAsync("ClearVotes");
    }

    private UserModel GetCurrentUserModel()
    {
        return _userModelList.FirstOrDefault(f => f.ConnectionId == this.Context.ConnectionId);
    }

    private async Task SetAreVotesBeingShowed(bool isVotesBeingShowed)
    {
        await _semaphoreSlim.WaitAsync();

        _areVotesBeingShowed = isVotesBeingShowed;

        _semaphoreSlim.Release();
    }

    private async Task DisconnectUser(Exception? exception = null)
    {
        var userModel = GetCurrentUserModel();

        if (userModel != null)
        {
            await _semaphoreSlim.WaitAsync();

            userModel = GetCurrentUserModel();

            if (userModel != null)
            {
                _userModelList.Remove(userModel);
            }

            _semaphoreSlim.Release();

            await this.Clients.Others.SendAsync("UserDisconnected", userModel);
        }

        await this.Clients.Others.SendAsync("SetVoteResult", GetVoteModel());

        await base.OnDisconnectedAsync(exception ?? new Exception());
    }

    private VoteModel GetVoteModel()
    {
        var voteModel = new VoteModel(_userModelList);

        return voteModel;
    }
}