using System.Collections.Concurrent;
using PointingPoker.Models;
using Serilog;

namespace PointingPoker.Services;

/// <summary>
/// Background service that periodically cleans up inactive sessions to prevent memory leaks.
/// </summary>
public class SessionCleanupService : BackgroundService
{
    private static ConcurrentDictionary<string, GroupModel> _groupsBySession = new();
    private static ConcurrentDictionary<string, DateTime> _lastActivity = new();

    // Configuration
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(1);
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromDays(1);

    /// <summary>
    /// Registers the groups dictionary for monitoring.
    /// This should be called once during application startup.
    /// </summary>
    public static void RegisterGroups(ConcurrentDictionary<string, GroupModel> groups)
    {
        _groupsBySession = groups ?? throw new ArgumentNullException(nameof(groups));
    }

    /// <summary>
    /// Updates the last activity timestamp for a session.
    /// Call this whenever there's activity in a session.
    /// </summary>
    public static void UpdateActivity(string sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            _lastActivity.AddOrUpdate(sessionId, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Removes the activity tracking for a session.
    /// Call this when a session is removed.
    /// </summary>
    public static void RemoveActivity(string sessionId)
    {
        _lastActivity.TryRemove(sessionId, out _);
    }

    /// <summary>
    /// Gets the count of active sessions.
    /// </summary>
    public static int ActiveSessionCount => _groupsBySession.Count;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Session cleanup service started. Running every {Interval} hours.",
            _cleanupInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                Log.Information("Checking inactive sessions...");

                await CleanupInactiveSessionsAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during session cleanup");
            }
        }
    }

    private Task CleanupInactiveSessionsAsync()
    {
        var now = DateTime.UtcNow;
        var removedSessions = 0;
        var removedUsers = 0;

        foreach (var sessionId in _groupsBySession.Keys.ToList())
        {
            // Check if session has timed out due to inactivity
            if (_lastActivity.TryGetValue(sessionId, out var lastActivity))
            {
                if (now - lastActivity > _sessionTimeout)
                {
                    if (_groupsBySession.TryRemove(sessionId, out var group))
                    {
                        removedUsers += group.Users.Count;
                        _lastActivity.TryRemove(sessionId, out _);
                        removedSessions++;
                        Log.Information("Session {SessionId} removed due to inactivity (last activity: {LastActivity})",
                            sessionId, lastActivity);
                    }
                }
            }
            else
            {
                // No activity recorded, add now
                _lastActivity.TryAdd(sessionId, now);
            }
        }

        if (removedSessions > 0)
        {
            Log.Information(
                "Cleanup complete: removed {Sessions} inactive sessions with {Users} users. Current sessions: {Current}",
                removedSessions, removedUsers, _groupsBySession.Count);
        }
        else
        {
            Log.Information("No inactive session removed.");
        }

        return Task.CompletedTask;
    }
}