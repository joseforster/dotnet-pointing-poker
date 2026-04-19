using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;
using PointingPoker.Enums;
using PointingPoker.Hubs;
using PointingPoker.Models;
using PointingPoker.Services;
using Serilog;

public class PointingPokerHub : Hub
{
    private static readonly ConcurrentDictionary<string, GroupModel> _groupsBySession = new();

    public PointingPokerHub()
    {
        SessionCleanupService.RegisterGroups(_groupsBySession);    
    }
    
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

            Log.Information("{Username} has joined the session {SessionId}.", newUserModel.Username, sessionId);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        await Clients.Caller.SendAsync("SetUserList", groupModel.GetUsersExcept(Context.ConnectionId),
            groupModel.AreVotesBeingShowed);

        if (groupModel.AreVotesBeingShowed)
        {
            var voteModel = groupModel.GetVoteModel();

            await Clients.Group(sessionId).SendAsync("SetVoteResult", voteModel);
            await Clients.Clients(groupModel.Watchers).SendAsync("SetVoteResultOnWatchSession", voteModel);
        }

        LogSessionCount();
        
        SessionCleanupService.UpdateActivity(sessionId);
    }

    private static void LogSessionCount()
    {
        Log.Information("Currently {GroupCount} sessions and {UsersCount} users.", _groupsBySession.Count,
            _groupsBySession.Values.Sum(s => s.Users.Count));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception ?? new Exception());
    }

    public async Task OnUserVoted(string userVote)
    {
        var sessionId = await GetCookieValue(EnumCustomClaimType.Session);
        var guid = await GetCookieValue(EnumCustomClaimType.Guid);

        if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(guid))
        {
            return;
        }

        if (!_groupsBySession.ContainsKey(sessionId))
        {
            return;
        }
        
        var groupModel = GetGroupModelBySession(sessionId);
        
        var userModel = groupModel.GetUserModelByGuid(guid);

        if (userModel == null)
        {
            return;
        }

        userModel.SetCurrentVote(userVote);

        if (groupModel.AreVotesBeingShowed)
        {
            await Clients.GroupExcept(sessionId, this.Context.ConnectionId)
                .SendAsync("UserHasVotedWithShowedVotes", userModel);

            var voteModel = groupModel.GetVoteModel();

            await Clients.Group(sessionId).SendAsync("SetVoteResult", voteModel);
            await Clients.Clients(groupModel.Watchers).SendAsync("SetVoteResultOnWatchSession", voteModel);
        }
        else
        {
            await Clients.GroupExcept(sessionId, this.Context.ConnectionId)
                .SendAsync("UserHasVoted", Context.ConnectionId);
        }
        
        SessionCleanupService.UpdateActivity(sessionId);
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

        var voteModel = groupModel.GetVoteModel();

        await Clients.Group(sessionId).SendAsync("ShowVotes", groupModel.Users);
        await Clients.Group(sessionId).SendAsync("SetVoteResult", voteModel);
        await Clients.Clients(groupModel.Watchers).SendAsync("SetVoteResultOnWatchSession", voteModel);
        
        SessionCleanupService.UpdateActivity(sessionId);
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
        await Clients.Clients(groupModel.Watchers).SendAsync("ClearVotesOnWatchSession");
        
        SessionCleanupService.UpdateActivity(sessionId);
    }

    public async Task ExitSession()
    {
        var sessionId = await GetCookieValue(EnumCustomClaimType.Session);
        var guid = await GetCookieValue(EnumCustomClaimType.Guid);

        if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(guid))
        {
            return;
        }

        var connectionId = Context.ConnectionId;

        var groupModel = GetGroupModelBySession(sessionId);

        var userModel = groupModel.GetUserModelByGuid(guid);

        await RemoveUserFromSession(connectionId, sessionId, userModel);
        
        RateLimitFilter.RemoveConnection(connectionId);
    }

    private async Task RemoveUserFromSession(string connectionId, string sessionId, UserModel userModel)
    {
        var groupModel = GetGroupModelBySession(sessionId);
        
        await Groups.RemoveFromGroupAsync(connectionId, sessionId);

        if (userModel == null)
        {
            return;
        }
        
        SessionCleanupService.UpdateActivity(sessionId);

        if (DoesSessionExist(sessionId))
        {
            groupModel.Users.Remove(userModel);

            if (!groupModel.Users.Any())
            {
                _groupsBySession.TryRemove(sessionId, out _);
                SessionCleanupService.RemoveActivity(sessionId);

                Log.Information("Session {SessionId} will be removed, {Username} was the last user.", sessionId,
                    userModel.Username);
            }
            else
            {
                if (groupModel.AreVotesBeingShowed)
                {
                    var voteModel = groupModel.GetVoteModel();

                    await Clients.GroupExcept(sessionId, connectionId)
                        .SendAsync("SetVoteResult", voteModel);

                    await Clients.Clients(groupModel.Watchers).SendAsync("SetVoteResultOnWatchSession", voteModel);
                }
            }
        }

        await Clients.Group(sessionId).SendAsync("UserDisconnected", userModel);

        Log.Information("User {Username} disconnected from session {SessionId}.", userModel.Username, sessionId);

        LogSessionCount();
        
        RateLimitFilter.RemoveConnection(connectionId);
    }

    public static bool DoesSessionExist(string sessionId)
    {
        return _groupsBySession.ContainsKey(sessionId);
    }

    public async Task AddWatcherToSession(string sessionToWatch)
    {
        if (!_groupsBySession.ContainsKey(sessionToWatch))
        {
            await Clients.Caller.SendAsync("SessionToWatchError", "No session found.");
            return;
        }

        var currentSession = await GetCookieValue(EnumCustomClaimType.Session);

        if (currentSession == sessionToWatch)
        {
            await Clients.Caller.SendAsync("SessionToWatchError",
                "Session to watch must be different from current session.");
            return;
        }

        RemoveWatcherFromOtherSessions();

        var groupModel = GetGroupModelBySession(sessionToWatch);

        groupModel.Watchers.Add(Context.ConnectionId);

        await Clients.Caller.SendAsync("SessionToWatchConnected", sessionToWatch);

        if (groupModel.AreVotesBeingShowed)
        {
            await Clients.Caller.SendAsync("SetVoteResultOnWatchSession", groupModel.GetVoteModel());
        }
    }

    public async Task KickUserFromSession(string connectionId)
    {
        var session = await GetCookieValue(EnumCustomClaimType.Session);
        
        var groupModel = GetGroupModelBySession(session);
        
        var userWhoKicked = groupModel.Users.Single(s => s.ConnectionId == Context.ConnectionId);
        var userThatWasKicked = groupModel.Users.Single(s => s.ConnectionId == connectionId);
        
        await RemoveUserFromSession(connectionId, session, userThatWasKicked);
        
        await Clients.Client(connectionId).SendAsync("KickedFromSession");
        
        await Clients.GroupExcept(session, connectionId, Context.ConnectionId).SendAsync("UserKickedFromSession",  userWhoKicked.Username, userThatWasKicked.Username);
        
        await OnClearVotes();
        
        SessionCleanupService.UpdateActivity(session);
    }

    private void RemoveWatcherFromOtherSessions()
    {
        var currentWatchingSessions = _groupsBySession.Where(wh => wh.Value.Watchers.Contains(Context.ConnectionId));

        if (currentWatchingSessions.Any())
        {
            foreach (var session in currentWatchingSessions)
            {
                session.Value.Watchers.Remove(Context.ConnectionId);
            }
        }
    }

    private async Task<string> GetCookieValue(EnumCustomClaimType enumCustomClaimType)
    {
        var claim = Context.User.Claims.FirstOrDefault(f => f.Type == enumCustomClaimType.ToString());

        if (claim == null)
        {
            await this.Context.GetHttpContext().SignOutAsync();

            Log.Information("Did not found cookie {Claim} on user {Username}.", enumCustomClaimType.ToString(),
                Context.User.Identity.Name);

            return string.Empty;
        }

        return claim.Value;
    }

    private static  GroupModel GetGroupModelBySession(string sessionId)
    {
        return _groupsBySession.GetOrAdd(sessionId, _ => new GroupModel());
    }
}