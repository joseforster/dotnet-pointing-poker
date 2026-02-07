namespace PointingPoker.Models;

public class GroupModel
{
    public bool AreVotesBeingShowed { get; set; } = false;

    public List<UserModel> Users { get; private set; } = new();

    public List<string> Watchers { get; private set; } = new();

    public void Reset()
    {
        this.AreVotesBeingShowed = false;
        this.Users.Clear();
    }

    public VoteModel GetVoteModel()
    {
        return new VoteModel(Users);
    }

    public void ClearVotes()
    {
        this.Users.ForEach(fe => fe.SetCurrentVote(string.Empty));
    }

    public IEnumerable<UserModel> GetUsersExcept(string connectionId)
    {
        return Users.Where(wh => wh.ConnectionId != connectionId);
    }

    public UserModel GetUserModelByGuid(string guid)
    {
        return this.Users.FirstOrDefault(f => f.Guid == guid);
    }

    public bool UserExistsByGuid(string guid)
    {
        return Users.Any(f => f.Guid == guid);
    }
}