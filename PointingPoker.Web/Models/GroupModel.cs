namespace PointingPoker.Models;

public class GroupModel
{
    public bool AreVotesBeingShowed { get; set; } = false;

    private List<UserModel> _users = new();

    public void Reset()
    {
        this.AreVotesBeingShowed = false;
        this._users.Clear();
    }
    
    public VoteModel GetVoteModel()
    {
        return new VoteModel(_users);
    }


    public void AddUser(UserModel userModel)
    {
        this._users.Add(userModel);
    }

    public void RemoveUser(UserModel user)
    {
        _users.Remove(user);
    }

    public bool IsEmpty()
    {
        return _users.Count == 0;
    }

    public void ClearVotes()
    {
        this._users.ForEach(fe => fe.SetCurrentVote(string.Empty));
    }

    public IEnumerable<UserModel> GetUsersExcept(string connectionId)
    {
        return _users.Where(wh => wh.ConnectionId != connectionId);
    }
    
    public UserModel GetUserModelByGuid(string guid)
    {
        return this._users.First(f => f.Guid == guid);
    }
    
    public UserModel GetUserModelByConnection(string connectionId)
    {
        return this._users.First(f => f.ConnectionId == connectionId);
    }

    public bool UserExistsByGuid(string guid)
    {
        return _users.Any(f => f.Guid == guid);
    }
    
    public List<UserModel> GetUsers()
    {
        return this._users;
    }
}