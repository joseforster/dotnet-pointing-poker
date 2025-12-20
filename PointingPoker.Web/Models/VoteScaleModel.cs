public class VoteScaleModel
{
    public const int MINIMAL_VOTE = 0;
    public const int LIMIT_BETWEEN_LOW_MEDIUM_VOTE = 12;
    public const int LIMIT_BETWEEN_MEDIUM_HIGH_VOTE = 50;
    public const string UNDECIDED_VOTE = "?";
    
    private Dictionary<EnumVoteScale, List<int>> _voteScaleByMinMax = new()
    {
        {EnumVoteScale.Low, [MINIMAL_VOTE, LIMIT_BETWEEN_LOW_MEDIUM_VOTE] },
        {EnumVoteScale.Medium, [LIMIT_BETWEEN_LOW_MEDIUM_VOTE, LIMIT_BETWEEN_MEDIUM_HIGH_VOTE] },
        {EnumVoteScale.High, [LIMIT_BETWEEN_MEDIUM_HIGH_VOTE, int.MaxValue] },
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