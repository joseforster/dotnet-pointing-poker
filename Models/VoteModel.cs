public class VoteModel
{
    public VoteModel(IEnumerable<UserModel> userModels)
    {
        SetVoteResult(userModels);
        SetEnumVoteScale();
    }

    public string VoteResult { get; private set; }

    public EnumVoteScale VoteScale { get; private set; }

    private Dictionary<EnumVoteScale, List<int>> _voteScaleByMinMax = new()
    {
        {EnumVoteScale.Low, new List<int>(){ 0,4 } },
        {EnumVoteScale.Medium, new List<int>(){ 4,60 } },
        {EnumVoteScale.High, new List<int>(){ 60,int.MaxValue } },
    };

    private void SetVoteResult(IEnumerable<UserModel> userModels)
    {
        var usersThatVoted = userModels.Where(wh => wh.HasVoted);

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
        decimal voteResult;

        if (!string.IsNullOrEmpty(VoteResult) && decimal.TryParse(VoteResult, out voteResult))
        {
            foreach (var voteScale in _voteScaleByMinMax)
            {
                if (voteResult >= voteScale.Value[0] && voteResult < voteScale.Value[1])
                {
                    VoteScale = voteScale.Key;
                }
            }
        }
        else
        {
            VoteScale = EnumVoteScale.Empty;
        }
    }
}