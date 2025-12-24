using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;
using PointingPoker.Enums;
using PointingPoker.Models;
using Serilog;

public class PointingPokerHub : Hub
{
    private static readonly ConcurrentDictionary<string, GroupModel> _groupsBySession = new();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var sessionId = await GetCookieValue(EnumCustomClaimType.Session);
        var guid = await GetCookieValue(EnumCustomClaimType.Guid);

        if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(guid))
        {
            return;
        }

        var groupModel = GetGroupModelBySession(sessionId);

        if (groupModel.UserExistsByGuid(guid))
        {
            var currentUserModel = groupModel.GetUserModelByGuid(guid);

            await Groups.RemoveFromGroupAsync(currentUserModel.ConnectionId, sessionId);

            await Clients.GroupExcept(sessionId, Context.ConnectionId)
                .SendAsync("UserHasReconnected", currentUserModel.ConnectionId, Context.ConnectionId);

            currentUserModel.SetConnectionId(Context.ConnectionId);

            Log.Information("{Username} has reconnected in session {SessionId}.", currentUserModel.Username, sessionId);
        }
        else
        {
            var newUserModel = new UserModel(Context.ConnectionId, Context.User.Identity.Name, sessionId, guid);

            groupModel.Users.Add(newUserModel);

            await Clients.GroupExcept(sessionId, this.Context.ConnectionId)
                .SendAsync("NewUserHasConnected", newUserModel);

            Log.Information("{Username} has joined the session {SessionId}.",newUserModel.Username, sessionId);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        await Clients.Caller.SendAsync("SetUserList", groupModel.GetUsersExcept(Context.ConnectionId),
            groupModel.AreVotesBeingShowed);

        if (groupModel.AreVotesBeingShowed)
        {
            await Clients.All.SendAsync("SetVoteResult", groupModel.GetVoteModel());
        }
        
        LogSessionCount();
    }

    private static void LogSessionCount()
    {
        Log.Information("Currently {GroupCount} sessions and {UsersCount} users.", _groupsBySession.Count, _groupsBySession.Values.Sum(s => s.Users.Count));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception ?? new Exception());
    }

    public async Task OnUserVoted(string userVote)
    {
        var sessionId = await GetCookieValue(EnumCustomClaimType.Session);
        var guid =  await  GetCookieValue(EnumCustomClaimType.Guid);
        
        if (string.IsNullOrEmpty(sessionId) ||  string.IsNullOrEmpty(guid))
        {
            return;
        }

        var groupModel = GetGroupModelBySession(sessionId);

        var userModel = groupModel.GetUserModelByGuid(guid);

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
        var sessionId = await GetCookieValue(EnumCustomClaimType.Session);

        if (string.IsNullOrEmpty(sessionId))
        {
            return;
        }

        var groupModel = GetGroupModelBySession(sessionId);

        groupModel.AreVotesBeingShowed = true;

        await Clients.Group(sessionId).SendAsync("ShowVotes", groupModel.Users);
        await Clients.Group(sessionId).SendAsync("SetVoteResult", groupModel.GetVoteModel());
    }

    public async Task OnClearVotes()
    {
        var sessionId = await GetCookieValue(EnumCustomClaimType.Session);

        if (string.IsNullOrEmpty(sessionId))
        {
            return;
        }

        var groupModel = GetGroupModelBySession(sessionId);

        groupModel.AreVotesBeingShowed = false;

        groupModel.ClearVotes();

        await Clients.Group(sessionId).SendAsync("ClearVotes");
    }

    public async Task ExitSession()
    {
        var sessionId = await GetCookieValue(EnumCustomClaimType.Session);
        var guid = await  GetCookieValue(EnumCustomClaimType.Guid);

        if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(guid))
        {
            return;
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);

        var groupModel = GetGroupModelBySession(sessionId);

        var userModel = groupModel.GetUserModelByGuid(guid);

        if (userModel == null)
        {
            return;
        }
        
        if (DoesSessionExist(sessionId))
        {
            groupModel.Users.Remove(userModel);

            if (!groupModel.Users.Any())
            {
                _groupsBySession.TryRemove(sessionId, out _);
                
                Log.Information("Session {SessionId} will be removed, {Username} was the last user.", sessionId, userModel.Username);
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

        await Clients.Group(sessionId).SendAsync("UserDisconnected", userModel);
        
        Log.Information("User {Username} disconnected in session {SessionId}.", userModel.Username, sessionId);
        
        LogSessionCount();
    }

    public static bool DoesSessionExist(string sessionId)
    {
        return _groupsBySession.ContainsKey(sessionId);
    }

    private async Task<string> GetCookieValue(EnumCustomClaimType enumCustomClaimType)
    {
        var claim = Context.User.Claims.FirstOrDefault(f => f.Type == enumCustomClaimType.ToString());

        if (claim == null)
        {
            await this.Context.GetHttpContext().SignOutAsync();
            
            Log.Information("Did not found cookie {Claim} on user {Username}.", enumCustomClaimType.ToString(), Context.User.Identity.Name);
            
            return string.Empty;
        }
        
        return claim.Value;
    }

    private GroupModel GetGroupModelBySession(string sessionId)
    {
        return _groupsBySession.GetOrAdd(sessionId, new GroupModel());
    }
}