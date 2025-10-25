using System.Globalization;

public class UserHubModel
{
    public UserHubModel(string connectionId, string username)
    {
        ConnectionId = connectionId;
        Username = username;
    }

    private readonly object _lockObject = new object();

    public string ConnectionId { get; set; }

    public string Username { get; set; }

    public bool HasVoted => _currentVote != 0;

    private decimal _currentVote;

    public void SetCurrentVote(string vote)
    {
        lock (_lockObject)
        {
            decimal parsedVote;

            if (!decimal.TryParse(vote, out parsedVote))
            {
                parsedVote = 0;
            }

            this._currentVote = parsedVote;
        }
    }
}