public class UserModel
{
    public UserModel(string connectionId, string username)
    {
        ConnectionId = connectionId;
        Username = username;
    }

    public string ConnectionId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string CurrentVote { get; private set; } = string.Empty;

    public EnumVoteScale VoteScale { get; set; }

    public bool HasVoted => !string.IsNullOrEmpty(this.CurrentVote);

    private VoteScaleHelper _voteScaleHelper = new VoteScaleHelper();

    public void SetCurrentVote(string vote)
    {
        CurrentVote = vote;
        this.VoteScale = _voteScaleHelper.GetEnumVoteScale(vote);
    }
}