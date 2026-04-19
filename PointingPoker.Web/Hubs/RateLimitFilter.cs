using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace PointingPoker.Hubs;

/// <summary>
/// Rate limiting filter for SignalR hub methods.
/// Prevents spam by tracking method calls per connection.
/// </summary>
public class RateLimitFilter : IHubFilter
{
    // Rate limiting configuration
    private static readonly TimeSpan VoteRateLimitWindow = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ShowVotesRateLimitWindow = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ClearVotesRateLimitWindow = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan KickUserRateLimitWindow = TimeSpan.FromSeconds(5);

    private static readonly ConcurrentDictionary<string, RateLimitEntry> _rateLimits = new();

    private class RateLimitEntry
    {
        public DateTime LastVoteTime { get; set; } = DateTime.MinValue;
        public DateTime LastShowVotesTime { get; set; } = DateTime.MinValue;
        public DateTime LastClearVotesTime { get; set; } = DateTime.MinValue;
        public DateTime LastKickTime { get; set; } = DateTime.MinValue;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var connectionId = invocationContext.Context.ConnectionId;
        var methodName = invocationContext.HubMethodName;

        if (!CheckRateLimit(connectionId, methodName))
        {
            Log.Warning("Rate limit exceeded for {Method} from connection {ConnectionId}", methodName, connectionId);
            await invocationContext.Hub.Clients.Caller.SendAsync("RateLimitExceeded", "Please slow down.");
            return null;
        }

        return await next(invocationContext);
    }

    private bool CheckRateLimit(string connectionId, string methodName)
    {
        var entry = _rateLimits.GetOrAdd(connectionId, _ => new RateLimitEntry());
        var now = DateTime.UtcNow;

        return methodName switch
        {
            "OnUserVoted" => CheckAndUpdate(entry, nameof(entry.LastVoteTime), now, VoteRateLimitWindow),
            "OnShowVotes" => CheckAndUpdate(entry, nameof(entry.LastShowVotesTime), now, ShowVotesRateLimitWindow),
            "OnClearVotes" => CheckAndUpdate(entry, nameof(entry.LastClearVotesTime), now, ClearVotesRateLimitWindow),
            "KickUserFromSession" => CheckAndUpdate(entry, nameof(entry.LastKickTime), now, KickUserRateLimitWindow),
            _ => true // No rate limit for other methods
        };
    }

    private bool CheckAndUpdate(RateLimitEntry entry, string propertyName, DateTime now, TimeSpan window)
    {
        var lastTime = propertyName switch
        {
            nameof(entry.LastVoteTime) => entry.LastVoteTime,
            nameof(entry.LastShowVotesTime) => entry.LastShowVotesTime,
            nameof(entry.LastClearVotesTime) => entry.LastClearVotesTime,
            nameof(entry.LastKickTime) => entry.LastKickTime,
            _ => DateTime.MinValue
        };

        if (now - lastTime < window)
        {
            return false; // Rate limited
        }

        // Update the property
        switch (propertyName)
        {
            case nameof(entry.LastVoteTime):
                entry.LastVoteTime = now;
                break;
            case nameof(entry.LastShowVotesTime):
                entry.LastShowVotesTime = now;
                break;
            case nameof(entry.LastClearVotesTime):
                entry.LastClearVotesTime = now;
                break;
            case nameof(entry.LastKickTime):
                entry.LastKickTime = now;
                break;
        }

        return true;
    }

    /// <summary>
    /// Cleans up rate limit entries for a disconnected connection.
    /// </summary>
    public static void RemoveConnection(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
        {
            return;
        }
        
        _rateLimits.TryRemove(connectionId, out _);
    }
}
