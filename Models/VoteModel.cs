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
        var usersThatVoted = userModels.Where(wh => wh.HasVoted && decimal.TryParse(wh.CurrentVote, out var result));

        var userCount = usersThatVoted.Count();

        if (userCount != 0)
        {
            var voteSum = usersThatVoted.Sum(s => decimal.Parse(s.CurrentVote));

            VoteResult = (Math.Round(voteSum / userCount * 2, MidpointRounding.AwayFromZero) / 2).ToString();
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