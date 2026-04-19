using PointingPoker.Enums;

public class VoteScaleModel
{
    public const int MIN_VOTE = 0;
    public const int LIMIT_BETWEEN_LOW_MEDIUM_VOTE = 12;
    public const int LIMIT_BETWEEN_MEDIUM_HIGH_VOTE = 60;
    public const int MAX_VOTE = 100;
    public const string UNDECIDED_VOTE = "?";
    
    private Dictionary<EnumVoteScale, List<int>> _voteScaleByMinMax = new()
    {
        {EnumVoteScale.Low, [MIN_VOTE, LIMIT_BETWEEN_LOW_MEDIUM_VOTE] },
        {EnumVoteScale.Medium, [LIMIT_BETWEEN_LOW_MEDIUM_VOTE, LIMIT_BETWEEN_MEDIUM_HIGH_VOTE] },
        {EnumVoteScale.High, [LIMIT_BETWEEN_MEDIUM_HIGH_VOTE, MAX_VOTE + 1] }
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