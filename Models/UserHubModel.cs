using System.Globalization;

public class UserHubModel
{
    public UserHubModel(string connectionId, string username)
    {
        ConnectionId = connectionId;
        Username = username;
    }

    public string ConnectionId { get; set; }

    public string Username { get; set; } = string.Empty;

    public decimal CurrentVote { get; set; } = 0M;

    public bool HasVoted => CurrentVote != 0;

    public bool IsEmptyVote { get; set; }
}