using Microsoft.AspNetCore.SignalR;
using PointingPoker.Enums;

public class PointingPokerHub : Hub
{
    private static Dictionary<string, List<UserModel>> _userModelListBySession = new ();

    private static readonly SemaphoreSlim _semaphoreSlim = new (1, 1);

    private static Dictionary<string, bool> _areVotesBeingShowedBySession = new ();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var username = Context.GetHttpContext().User.Identity.Name!;
        var sessionId = GetSessionId();

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        if (!_userModelListBySession.ContainsKey(sessionId))
        {
            await _semaphoreSlim.WaitAsync();

            if (!_userModelListBySession.ContainsKey(sessionId))
            {
                _userModelListBySession[sessionId] = new List<UserModel>();
            }
            
            _semaphoreSlim.Release();
        }
        
        if (!_areVotesBeingShowedBySession.ContainsKey(sessionId))
        {
            await SetAreVotesBeingShowed(false, sessionId);
        }

        await Clients.Caller.SendAsync("SetUserList", _userModelListBySession[sessionId],
            _areVotesBeingShowedBySession[sessionId]);

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
            await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                .SendAsync("UserHasVotedWithShowedVotes", userModel);
            await Clients.Group(userModel.SessionId).SendAsync("SetVoteResult", GetVoteModel(userModel.SessionId));
        }
        else
        {
            await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                .SendAsync("UserHasVoted", Context.ConnectionId);
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
        return _userModelListBySession.SelectMany(s => s.Value)
            .FirstOrDefault(f => f.ConnectionId == Context.ConnectionId);
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
                if (_userModelListBySession[userModel.SessionId].Remove(userModel) &&
                    _userModelListBySession[userModel.SessionId].Count == 0)
                {
                    if (_userModelListBySession.Remove(userModel.SessionId))
                    {
                        _areVotesBeingShowedBySession.Remove(userModel.SessionId);
                    }
                }
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userModel.SessionId);

                await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                    .SendAsync("UserDisconnected", userModel);

                await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                    .SendAsync("SetVoteResult", GetVoteModel(userModel.SessionId));
            }
            
            _semaphoreSlim.Release();
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
        return Context.GetHttpContext().User.Claims.First(f => f.Type == nameof(EnumCustomClaimType.Session)).Value;
    }
}