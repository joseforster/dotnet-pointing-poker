public class VoteModel
{
    public VoteModel(IEnumerable<UserModel> userModels)
    {
        _voteScaleModel = new VoteScaleModel();
        SetVoteResult(userModels);
        SetEnumVoteScale();
    }

    public string VoteResult { get; private set; }

    public EnumVoteScale VoteScale { get; private set; }

    private readonly VoteScaleModel _voteScaleModel;


    private void SetVoteResult(IEnumerable<UserModel> userModels)
    {
        var usersThatVoted = userModels.Where(wh => wh.HasVoted);

        if (usersThatVoted.Any() && usersThatVoted.All(a => !a.IsVoteANumber))
        {
            VoteResult = VoteScaleModel.UNDECIDED_VOTE;
            return;
        }

        usersThatVoted = usersThatVoted.Where(wh => wh.IsVoteANumber);

        var userCount = usersThatVoted.Count();

        if (userCount != 0)
        {
            var voteSum = usersThatVoted.Sum(s => decimal.Parse(s.CurrentVote));

            var voteValue = voteSum / userCount;
            
            var roundedVoteValue = Math.Round(voteValue * 2, MidpointRounding.AwayFromZero) / 2;

            VoteResult = roundedVoteValue.ToString();
        }
        else
        {
            VoteResult = string.Empty;
        }
    }

    private void SetEnumVoteScale()
    {
        this.VoteScale = _voteScaleModel.GetEnumVoteScale(this.VoteResult);
    }
}