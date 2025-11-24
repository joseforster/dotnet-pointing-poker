using Microsoft.AspNetCore.SignalR;
using PointingPoker.Enums;

public class PointingPokerHub : Hub
{
    private static readonly Dictionary<string, List<UserModel>> _userModelListBySession = new();

    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private static readonly Dictionary<string, bool> _areVotesBeingShowedBySession = new();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var username = Context.GetHttpContext().User.Identity.Name!;
        var sessionId = GetSessionId();

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        if (!_userModelListBySession.ContainsKey(sessionId))
        {
            try
            {
                await _semaphoreSlim.WaitAsync();

                if (!_userModelListBySession.ContainsKey(sessionId))
                {
                    _userModelListBySession[sessionId] = new List<UserModel>();
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
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
            try
            {
                await _semaphoreSlim.WaitAsync();

                if (!_userModelListBySession[sessionId].Contains(userHubModel))
                {
                    _userModelListBySession[sessionId].Add(userHubModel);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        await Clients.GroupExcept(sessionId, this.Context.ConnectionId).SendAsync("NewUserHasConnected", userHubModel);

        if (_areVotesBeingShowedBySession[sessionId])
        {
            await Clients.Caller.SendAsync("SetVoteResult", GetVoteModel(sessionId));
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userModel = GetCurrentUserModel();

        if (userModel != null)
        {
            await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                .SendAsync("UserOffline", userModel);
        }

        await base.OnDisconnectedAsync(exception ?? new Exception());
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

        try
        {
            await _semaphoreSlim.WaitAsync();

            _userModelListBySession[sessionId].ForEach(fe => fe.SetCurrentVote(string.Empty));
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        await Clients.Group(sessionId).SendAsync("ClearVotes");
    }

    public async Task ExitSession()
    {
        var userModel = GetCurrentUserModel();

        if (userModel != null)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();

                userModel = GetCurrentUserModel();

                if (userModel != null)
                {
                    if (_userModelListBySession.ContainsKey(userModel.SessionId) &&
                        _userModelListBySession[userModel.SessionId].Remove(userModel) &&
                        _userModelListBySession[userModel.SessionId].Count == 0 &&
                        _userModelListBySession.Remove(userModel.SessionId))
                    {
                        _areVotesBeingShowedBySession.Remove(userModel.SessionId);
                    }

                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userModel.SessionId);

                    await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                        .SendAsync("UserDisconnected", userModel);

                    if (_areVotesBeingShowedBySession[userModel.SessionId])
                    {
                        await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                            .SendAsync("SetVoteResult", GetVoteModel(userModel.SessionId));
                    }
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }

    public static bool DoesSessionExist(string sessionId)
    {
        return _userModelListBySession.ContainsKey(sessionId);
    }

    private VoteModel GetVoteModel(string sessionId)
    {
        if (_userModelListBySession.ContainsKey(sessionId))
        {
            return new VoteModel(_userModelListBySession[sessionId]);
        }
        else
        {
            return new VoteModel(Enumerable.Empty<UserModel>());
        }
    }

    private string GetSessionId()
    {
        return Context.GetHttpContext().User.Claims.First(f => f.Type == nameof(EnumCustomClaimType.Session)).Value;
    }

    private async Task SetAreVotesBeingShowed(bool isVotesBeingShowed, string sessionId)
    {
        try
        {
            await _semaphoreSlim.WaitAsync();

            _areVotesBeingShowedBySession[sessionId] = isVotesBeingShowed;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private UserModel GetCurrentUserModel()
    {
        return _userModelListBySession.SelectMany(s => s.Value)
            .FirstOrDefault(f => f.ConnectionId == Context.ConnectionId);
    }
}