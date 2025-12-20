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

        var groupModel = GetGroupModelBySession(sessionId);

        if (isReconnect(sessionId, guid))
        {
            var userModel = groupModel.GetUserModelByGuid(guid);

            await Groups.RemoveFromGroupAsync(userModel.ConnectionId, sessionId);

            await Clients.GroupExcept(sessionId, Context.ConnectionId)
                .SendAsync("UserHasReconnected", userModel.ConnectionId, Context.ConnectionId);

            userModel.SetConnectionId(Context.ConnectionId);
        }
        else
        {
            var userHubModel = new UserModel(Context.ConnectionId, Context.User.Identity.Name, sessionId, guid);

            groupModel.AddUser(userHubModel);

            await Clients.GroupExcept(sessionId, this.Context.ConnectionId)
                .SendAsync("NewUserHasConnected", userHubModel);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        await Clients.Caller.SendAsync("SetUserList", groupModel.GetUsersExcept(Context.ConnectionId),
            groupModel.AreVotesBeingShowed);

        if (groupModel.AreVotesBeingShowed)
        {
            await Clients.All.SendAsync("SetVoteResult", groupModel.GetVoteModel());
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception ?? new Exception());
    }

    public async Task OnUserVoted(string userVote)
    {
        var sessionId = GetCookieValue<string>(EnumCustomClaimType.Session);

        var groupModel = GetGroupModelBySession(sessionId);

        var userModel = groupModel.GetUserModelByConnection(this.Context.ConnectionId);

        userModel.SetCurrentVote(userVote);

        if (groupModel.AreVotesBeingShowed)
        {
            await Clients.GroupExcept(sessionId, this.Context.ConnectionId)
                .SendAsync("UserHasVotedWithShowedVotes", userModel);

            await Clients.Group(sessionId).SendAsync("SetVoteResult", groupModel.GetVoteModel());
        }
        else
        {
            await Clients.GroupExcept(sessionId, this.Context.ConnectionId)
                .SendAsync("UserHasVoted", Context.ConnectionId);
        }
    }

    public async Task OnShowVotes()
    {
        var sessionId = GetCookieValue<string>(EnumCustomClaimType.Session);

        var groupModel = GetGroupModelBySession(sessionId);

        groupModel.AreVotesBeingShowed = true;

        await Clients.Group(sessionId).SendAsync("ShowVotes", groupModel.GetUsers());
        await Clients.Group(sessionId).SendAsync("SetVoteResult", groupModel.GetVoteModel());
    }

    public async Task OnClearVotes()
    {
        var sessionId = GetCookieValue<string>(EnumCustomClaimType.Session);

        var groupModel = GetGroupModelBySession(sessionId);

        groupModel.AreVotesBeingShowed = false;

        groupModel.ClearVotes();

        await Clients.Group(sessionId).SendAsync("ClearVotes");
    }

    public async Task ExitSession()
    {
        var sessionId = GetCookieValue<string>(EnumCustomClaimType.Session);

        var groupModel = GetGroupModelBySession(sessionId);

        var userModel = groupModel.GetUserModelByConnection(this.Context.ConnectionId);
        
        if (DoesSessionExist(userModel.SessionId))
        {
            groupModel.RemoveUser(userModel);

            if (groupModel.IsEmpty())
            {
                _groupsBySession.TryRemove(userModel.SessionId, out _);
            }
            else
            {
                if (groupModel.AreVotesBeingShowed)
                {
                    await Clients.GroupExcept(sessionId, this.Context.ConnectionId)
                        .SendAsync("SetVoteResult", groupModel.GetVoteModel());
                }
            }
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);

        await Clients.GroupExcept(sessionId, this.Context.ConnectionId)
            .SendAsync("UserDisconnected", userModel);
    }

    public static bool DoesSessionExist(string sessionId)
    {
        return _groupsBySession.ContainsKey(sessionId);
    }

    private T GetCookieValue<T>(EnumCustomClaimType enumCustomClaimType)
    {
        return (T)Convert.ChangeType(Context.User.Claims.First(f => f.Type == enumCustomClaimType.ToString()).Value,
            typeof(T));
    }


    private bool isReconnect(string sessionId, string guid)
    {
        return _groupsBySession.TryGetValue(sessionId, out GroupModel groupModel) && groupModel.UserExistsByGuid(guid);
    }

    private GroupModel GetGroupModelBySession(string sessionId)
    {
        return _groupsBySession.GetOrAdd(sessionId, new GroupModel());
    }
}