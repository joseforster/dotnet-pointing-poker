namespace PointingPoker.Models;

public class GroupModel
{
    public bool AreVotesBeingShowed { get; private set; } = false;

    public List<UserModel> Users { get; set; } = new();

    public void Reset()
    {
        this.AreVotesBeingShowed = false;
        this.Users.Clear();
    }

    public void RemoveUser(UserModel user)
    {
        Users.Remove(user);
    }

    public bool IsEmpty()
    {
        return Users.Count == 0;
    }

    public void SetAreVotesBeingShowed(bool  areVotesBeingShowed)
    {
        this.AreVotesBeingShowed = areVotesBeingShowed;
    }

    public void ClearVotes()
    {
        this.Users.ForEach(fe => fe.SetCurrentVote(string.Empty));
    }

    public IEnumerable<UserModel> GetUsersExcept(string connectionId)
    {
        return Users.Where(wh => wh.ConnectionId != connectionId);
    }
}