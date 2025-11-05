public class VoteScaleHelper
{
    private const string UNDECIDED_VOTE = "?";
    private Dictionary<EnumVoteScale, List<int>> _voteScaleByMinMax = new()
    {
        {EnumVoteScale.Low, new List<int>(){ 0, 12 } },
        {EnumVoteScale.Medium, new List<int>(){ 12, 60 } },
        {EnumVoteScale.High, new List<int>(){ 60, int.MaxValue } },
    };

    public EnumVoteScale GetEnumVoteScale(string vote)
    {
        if (!string.IsNullOrEmpty(vote) && vote.Equals(UNDECIDED_VOTE))
        {
            return EnumVoteScale.Undecided;
        }

        EnumVoteScale voteScaleResult = EnumVoteScale.Empty;

        decimal voteResult;

        if (string.IsNullOrEmpty(vote) || !decimal.TryParse(vote, out voteResult))
        {
            return voteScaleResult;
        }

        foreach (var voteScale in _voteScaleByMinMax)
        {
            if (voteResult >= voteScale.Value[0] && voteResult < voteScale.Value[1])
            {
                voteScaleResult = voteScale.Key;
                break;
            }
        }

        return voteScaleResult;
    }
}