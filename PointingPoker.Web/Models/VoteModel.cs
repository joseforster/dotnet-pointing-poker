public class VoteModel
{
    public VoteModel(IEnumerable<UserModel> userModels)
    {
        SetVoteResult(userModels);
        SetEnumVoteScale();
    }

    public string VoteResult { get; private set; }

    public EnumVoteScale VoteScale { get; private set; }

    private VoteScaleHelper _voteScaleHelper;


    private void SetVoteResult(IEnumerable<UserModel> userModels)
    {
        var usersThatVoted = userModels.Where(wh => wh.HasVoted);

        if (usersThatVoted.Any() && usersThatVoted.All(a => !a.IsVoteAnNumber))
        {
            VoteResult = "?";
            return;
        }

        usersThatVoted = usersThatVoted.Where(wh => wh.IsVoteAnNumber);

        var userCount = usersThatVoted.Count();

        if (userCount != 0)
        {
            var voteSum = usersThatVoted.Sum(s => decimal.Parse(s.CurrentVote));

            VoteResult = Math.Round(voteSum / userCount, 1).ToString();
        }
        else
        {
            VoteResult = string.Empty;
        }
    }

    private void SetEnumVoteScale()
    {
        var voteScaleHelper = new VoteScaleHelper();

        this.VoteScale = voteScaleHelper.GetEnumVoteScale(this.VoteResult);
    }
}