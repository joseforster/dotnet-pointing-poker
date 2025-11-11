using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

public class PointingPokerHub : Hub
{
    private static Dictionary<string, List<UserModel>> _userModelListBySession = new Dictionary<string, List<UserModel>>();
    private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    private static Dictionary<string, bool> _areVotesBeingShowedBySession = new Dictionary<string, bool>();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var username = Context.GetHttpContext().User.Identity.Name!;
        var sessionId = Context.GetHttpContext().User.Claims.First(f => f.Type == "Session").Value;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        await Clients.Caller.SendAsync("SetUserList", _userModelListBySession[sessionId], _areVotesBeingShowedBySession);

        var userHubModel = new UserModel(Context.ConnectionId, username, sessionId);

        if (!_userModelListBySession[sessionId].Contains(userHubModel))
        {
            await _semaphoreSlim.WaitAsync();

            if (!_userModelListBySession[sessionId].Contains(userHubModel))
            {
                _userModelListBySession[sessionId].Add(userHubModel);
            }

            _semaphoreSlim.Release();
        }
        
        await Clients.GroupExcept(sessionId, this.Context.ConnectionId).SendAsync("NewUserHasConnected", userHubModel);

        if (_areVotesBeingShowedBySession[sessionId])
        {
            await Clients.Caller.SendAsync("SetVoteResult", GetVoteModel(sessionId));
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
        var userModel = GetCurrentUserModel();

        userModel.SetCurrentVote(userVote);

        if (_areVotesBeingShowedBySession[userModel.SessionId])
        {
            await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId).SendAsync("UserHasVotedWithShowedVotes", userModel);
            await Clients.Group(userModel.SessionId).SendAsync("SetVoteResult", GetVoteModel(userModel.SessionId));
        }
        else
        {
            await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId).SendAsync("UserHasVoted", Context.ConnectionId);
        }
    }

    public async Task OnShowVotes()
    {
        string sessionId = GetSessionId();
        
        await SetAreVotesBeingShowed(true, sessionId);
        
        await Clients.Group(sessionId).SendAsync("ShowVotes", _userModelListBySession[sessionId]);
        await Clients.Group(sessionId).SendAsync("SetVoteResult", GetVoteModel(sessionId));
    }

    public async Task OnClearVotes()
    {
        string sessionId = GetSessionId();
        
        await SetAreVotesBeingShowed(false, sessionId);

        await _semaphoreSlim.WaitAsync();

        _userModelListBySession[sessionId].ForEach(fe => fe.SetCurrentVote(string.Empty));

        _semaphoreSlim.Release();

        await Clients.Group(sessionId).SendAsync("ClearVotes");
    }

    private UserModel GetCurrentUserModel()
    {
        return _userModelListBySession.SelectMany(s => s.Value).FirstOrDefault(f => f.ConnectionId == Context.ConnectionId);
    }

    private async Task SetAreVotesBeingShowed(bool isVotesBeingShowed, string sessionId)
    {
        await _semaphoreSlim.WaitAsync();

        _areVotesBeingShowedBySession[sessionId] = isVotesBeingShowed;

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
                _userModelListBySession[userModel.SessionId].Remove(userModel);
            }

            _semaphoreSlim.Release();

            await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId).SendAsync("UserDisconnected", userModel);
            
            await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId).SendAsync("SetVoteResult", GetVoteModel(userModel.SessionId));
        }

        await base.OnDisconnectedAsync(exception ?? new Exception());
    }

    private VoteModel GetVoteModel(string sessionId)
    {
        var voteModel = new VoteModel(_userModelListBySession[sessionId]);

        return voteModel;
    }

    private string GetSessionId()
    {
        return Context.GetHttpContext().User.Claims.First(f => f.Type == "Session").Value;
    }
}