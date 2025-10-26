using System.Globalization;

public class UserHubModel
{
    public UserHubModel(string connectionId, string username)
    {
        ConnectionId = connectionId;
        Username = username;
    }

    public string ConnectionId { get; set; }

    public string Username { get; set; }

    public decimal CurrentVote { get; set; }

    public bool HasVoted => CurrentVote != 0;
}