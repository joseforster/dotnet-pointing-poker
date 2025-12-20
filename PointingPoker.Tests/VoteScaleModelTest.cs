namespace PointingPoker.Tests;

[TestClass]
public sealed class VoteScaleModelTest
{
    private readonly VoteScaleModel _voteScaleModel = new VoteScaleModel();

    [TestMethod]
    public void GetEnumVoteScale_WithHighVote_ReturnEnumVoteScaleHigh()
    {
        var valorInicial = VoteScaleModel.LIMIT_BETWEEN_MEDIUM_HIGH_VOTE;

        for (int i = 0; i < 10; i++)
        {
            var result = _voteScaleModel.GetEnumVoteScale(valorInicial.ToString());

            Assert.AreEqual(EnumVoteScale.High, result);

            valorInicial += 10;
        }
    }

    [TestMethod]
    public void GetEnumVoteScale_WithMediumVote_ReturnEnumVoteScaleMedium()
    {
        var valorInicial = VoteScaleModel.LIMIT_BETWEEN_LOW_MEDIUM_VOTE + 1;
        var valorFinal = VoteScaleModel.LIMIT_BETWEEN_MEDIUM_HIGH_VOTE -1;

        for (var i = valorInicial; i <= valorFinal; i++)
        {
            var result = _voteScaleModel.GetEnumVoteScale(i.ToString());

            Assert.AreEqual(EnumVoteScale.Medium, result);
        }
    }

    [TestMethod]
    public void GetEnumVoteScale_WithLowVote_ReturnEnumVoteScaleLow()
    {
        var valorInicial = VoteScaleModel.MINIMAL_VOTE;
        var valorFinal = VoteScaleModel.LIMIT_BETWEEN_LOW_MEDIUM_VOTE - 1;

        for (var i = valorInicial; i <= valorFinal; i++)
        {
            var result = _voteScaleModel.GetEnumVoteScale(i.ToString());

            Assert.AreEqual(EnumVoteScale.Low, result);
        }
    }

    [TestMethod]
    public void GetEnumVoteScale_WithUndecidedVote_ReturnEnumVoteScaleUndecided()
    {
        var result = _voteScaleModel.GetEnumVoteScale(VoteScaleModel.UNDECIDED_VOTE);

        Assert.AreEqual(EnumVoteScale.Undecided, result);
    }
    
    [TestMethod]
    public void GetEnumVoteScale_WithEmptyVote_ReturnEnumVoteScaleEmpty()
    {
        var result = _voteScaleModel.GetEnumVoteScale(string.Empty);

        Assert.AreEqual(EnumVoteScale.Empty, result);
    }
    
    [TestMethod]
    public void GetEnumVoteScale_WithNegativeVote_ReturnEnumVoteScaleEmpty()
    {
        var result = _voteScaleModel.GetEnumVoteScale("-1");

        Assert.AreEqual(EnumVoteScale.Empty, result);
    }
    
    [TestMethod]
    public void GetEnumVoteScale_WithInvalidVote_ReturnEnumVoteScaleEmpty()
    {
        var result = _voteScaleModel.GetEnumVoteScale("abc");

        Assert.AreEqual(EnumVoteScale.Empty, result);
    }
}