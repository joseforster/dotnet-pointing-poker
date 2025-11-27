public class UserModel
{
    public UserModel(string connectionId, string username, string sessionId, string guid)
    {
        ConnectionId = connectionId;
        Username = username;
        SessionId = sessionId;
        Guid = guid;
    }

    public string ConnectionId { get; private set; }

    public string Username { get; private set; }

    public string CurrentVote { get; private set; } = string.Empty;
    
    public string SessionId { get; private set; } = string.Empty;

    public EnumVoteScale VoteScale { get; private set; }

    public bool HasVoted => !string.IsNullOrEmpty(this.CurrentVote);
    
    public bool IsVoteAnNumber => decimal.TryParse(this.CurrentVote, out _);
    
    public string Guid  { get; private set; } = string.Empty;

    private VoteScaleHelper _voteScaleHelper = new VoteScaleHelper();

    public void SetCurrentVote(string vote)
    {
        CurrentVote = vote;
        VoteScale = _voteScaleHelper.GetEnumVoteScale(vote);
    }
}