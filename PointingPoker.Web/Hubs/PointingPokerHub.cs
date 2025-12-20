using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using PointingPoker.Enums;
using PointingPoker.Models;

public class PointingPokerHub : Hub
{
    private static readonly ConcurrentDictionary<string, GroupModel> _groupsBySession = new();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var sessionId = GetCookieValue<string>(EnumCustomClaimType.Session);

        var guid = GetCookieValue<string>(EnumCustomClaimType.Guid);

        var isReconnect = _groupsBySession.ContainsKey(sessionId) &&
                          _groupsBySession[sessionId].Users.Any(an => an.Guid == guid);

        if (isReconnect)
        {
            var userModel = _groupsBySession[sessionId].Users.First(f => f.Guid == guid);
            
            await Groups.RemoveFromGroupAsync(userModel.ConnectionId, sessionId);

            await Clients.GroupExcept(sessionId, Context.ConnectionId)
                .SendAsync("UserHasReconnected", userModel.ConnectionId, Context.ConnectionId);
            
            userModel.SetConnectionId(Context.ConnectionId);
        }
        else
        {
            if (!_groupsBySession.ContainsKey(sessionId))
            {
                _groupsBySession.TryAdd(sessionId, new GroupModel());
            }

            var userHubModel = new UserModel(Context.ConnectionId, Context.User.Identity.Name, sessionId, guid);
            
            if (!_groupsBySession[sessionId].Users.Contains(userHubModel))
            {
                _groupsBySession[sessionId].Users.Add(userHubModel);
            }
            
            await Clients.GroupExcept(sessionId, this.Context.ConnectionId)
                .SendAsync("NewUserHasConnected", userHubModel);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        
        await Clients.Caller.SendAsync("SetUserList", _groupsBySession[sessionId].GetUsersExcept(Context.ConnectionId), _groupsBySession[sessionId].AreVotesBeingShowed);

        if (_groupsBySession[sessionId].AreVotesBeingShowed)
        {
            await Clients.All.SendAsync("SetVoteResult", GetVoteModel(sessionId));
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception ?? new Exception());
    }

    public async Task OnUserVoted(string userVote)
    {
        var userModel = GetCurrentUserModel();

        if (userModel != null)
        {
            userModel.SetCurrentVote(userVote);

            if (_groupsBySession.ContainsKey(userModel.SessionId))
            {
                if (_groupsBySession[userModel.SessionId].AreVotesBeingShowed)
                {
                    await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                        .SendAsync("UserHasVotedWithShowedVotes", userModel);

                    await Clients.Group(userModel.SessionId)
                        .SendAsync("SetVoteResult", GetVoteModel(userModel.SessionId));
                }
                else
                {
                    await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                        .SendAsync("UserHasVoted", Context.ConnectionId);
                }
            }
        }
    }

    public async Task OnShowVotes()
    {
        var sessionId = GetCookieValue<string>(EnumCustomClaimType.Session);

        await SetAreVotesBeingShowed(true, sessionId);

        await Clients.Group(sessionId).SendAsync("ShowVotes", _groupsBySession[sessionId].Users);
        await Clients.Group(sessionId).SendAsync("SetVoteResult", GetVoteModel(sessionId));
    }

    public async Task OnClearVotes()
    {
        var sessionId = GetCookieValue<string>(EnumCustomClaimType.Session);

        await SetAreVotesBeingShowed(false, sessionId);

        _groupsBySession[sessionId].ClearVotes();

        await Clients.Group(sessionId).SendAsync("ClearVotes");
    }

    public async Task ExitSession()
    {
        var userModel = GetCurrentUserModel();

        if (userModel != null)
        {
            if (DoesSessionExist(userModel.SessionId))
            {
                _groupsBySession[userModel.SessionId].RemoveUser(userModel);

                if (_groupsBySession[userModel.SessionId].IsEmpty())
                {
                    _groupsBySession.TryRemove(userModel.SessionId, out _);
                }
                else
                {
                    if (_groupsBySession[userModel.SessionId].AreVotesBeingShowed)
                    {
                        await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                            .SendAsync("SetVoteResult", GetVoteModel(userModel.SessionId));
                    }
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userModel.SessionId);

            await Clients.GroupExcept(userModel.SessionId, this.Context.ConnectionId)
                .SendAsync("UserDisconnected", userModel);
        }
    }

    public static bool DoesSessionExist(string sessionId)
    {
        return _groupsBySession.ContainsKey(sessionId);
    }

    private VoteModel GetVoteModel(string sessionId)
    {
        if (!DoesSessionExist(sessionId))
        {
            return new VoteModel(Enumerable.Empty<UserModel>());
        }

        return new VoteModel(_groupsBySession[sessionId].Users);
    }

    private T GetCookieValue<T>(EnumCustomClaimType enumCustomClaimType)
    {
        return (T)Convert.ChangeType(Context.User.Claims.First(f => f.Type == enumCustomClaimType.ToString()).Value,
            typeof(T));
    }

    private async Task SetAreVotesBeingShowed(bool isVotesBeingShowed, string sessionId)
    {
        _groupsBySession[sessionId].SetAreVotesBeingShowed(isVotesBeingShowed);
    }

    private UserModel GetCurrentUserModel()
    {
        return _groupsBySession.SelectMany(s => s.Value.Users)
            .FirstOrDefault(f => f.ConnectionId == Context.ConnectionId);
    }
}