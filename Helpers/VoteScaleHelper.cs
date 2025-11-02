public class VoteScaleHelper
{
    private Dictionary<EnumVoteScale, List<int>> _voteScaleByMinMax = new()
    {
        {EnumVoteScale.Low, new List<int>(){ 0, 4 } },
        {EnumVoteScale.Medium, new List<int>(){ 4, 60 } },
        {EnumVoteScale.High, new List<int>(){ 60, int.MaxValue } },
    };

    public EnumVoteScale GetEnumVoteScale(string vote)
    {
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