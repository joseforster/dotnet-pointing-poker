namespace PointingPoker.Tests;

[TestClass]
public sealed class VoteScaleHelperTest
{
    private readonly VoteScaleHelper voteScaleHelper = new VoteScaleHelper();

    [TestMethod]
    public void GetEnumVoteScale_WithHighVote_ReturnEnumVoteScaleHigh()
    {
        var valorInicial = VoteScaleHelper.LIMIT_BETWEEN_MEDIUM_HIGH_VOTE;

        for (int i = 0; i < 10; i++)
        {
            var result = voteScaleHelper.GetEnumVoteScale(valorInicial.ToString());

            Assert.AreEqual(EnumVoteScale.High, result);

            valorInicial += 10;
        }
    }

    [TestMethod]
    public void GetEnumVoteScale_WithMediumVote_ReturnEnumVoteScaleMedium()
    {
        var valorInicial = VoteScaleHelper.LIMIT_BETWEEN_LOW_MEDIUM_VOTE + 1;
        var valorFinal = VoteScaleHelper.LIMIT_BETWEEN_MEDIUM_HIGH_VOTE -1;

        for (var i = valorInicial; i <= valorFinal; i++)
        {
            var result = voteScaleHelper.GetEnumVoteScale(i.ToString());

            Assert.AreEqual(EnumVoteScale.Medium, result);
        }
    }

    [TestMethod]
    public void GetEnumVoteScale_WithLowVote_ReturnEnumVoteScaleLow()
    {
        var valorInicial = VoteScaleHelper.MINIMAL_VOTE;
        var valorFinal = VoteScaleHelper.LIMIT_BETWEEN_LOW_MEDIUM_VOTE - 1;

        for (var i = valorInicial; i <= valorFinal; i++)
        {
            var result = voteScaleHelper.GetEnumVoteScale(i.ToString());

            Assert.AreEqual(EnumVoteScale.Low, result);
        }
    }

    [TestMethod]
    public void GetEnumVoteScale_WithUndecidedVote_ReturnEnumVoteScaleUndecided()
    {
        var result = voteScaleHelper.GetEnumVoteScale(VoteScaleHelper.UNDECIDED_VOTE);

        Assert.AreEqual(EnumVoteScale.Undecided, result);
    }
    
    [TestMethod]
    public void GetEnumVoteScale_WithEmptyVote_ReturnEnumVoteScaleEmpty()
    {
        var result = voteScaleHelper.GetEnumVoteScale(string.Empty);

        Assert.AreEqual(EnumVoteScale.Empty, result);
    }
    
    [TestMethod]
    public void GetEnumVoteScale_WithNegativeVote_ReturnEnumVoteScaleEmpty()
    {
        var result = voteScaleHelper.GetEnumVoteScale("-1");

        Assert.AreEqual(EnumVoteScale.Empty, result);
    }
    
    [TestMethod]
    public void GetEnumVoteScale_WithInvalidVote_ReturnEnumVoteScaleEmpty()
    {
        var result = voteScaleHelper.GetEnumVoteScale("abc");

        Assert.AreEqual(EnumVoteScale.Empty, result);
    }
}