using System.Globalization;

public class UserModel
{
    public UserModel(string connectionId, string username)
    {
        ConnectionId = connectionId;
        Username = username;
    }

    public string ConnectionId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string CurrentVote { get; set; } = string.Empty;

    public bool HasVoted => !string.IsNullOrEmpty(this.CurrentVote) && decimal.TryParse(this.CurrentVote, out decimal result);
}