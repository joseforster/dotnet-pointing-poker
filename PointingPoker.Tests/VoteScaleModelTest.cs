using PointingPoker.Enums;

namespace PointingPoker.Tests;

[TestClass]
public sealed class VoteScaleModelTest
{
    private readonly VoteScaleModel _voteScaleModel = new VoteScaleModel();

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
        var valorInicial = VoteScaleModel.MIN_VOTE;
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

    [TestMethod]
    public void GetEnumVoteScale_WithExactLowBoundary_ReturnsLow()
    {
        var result = _voteScaleModel.GetEnumVoteScale(VoteScaleModel.MIN_VOTE.ToString());

        Assert.AreEqual(EnumVoteScale.Low, result);
    }

    [TestMethod]
    public void GetEnumVoteScale_WithJustBelowLowMediumBoundary_ReturnsLow()
    {
        var boundary = VoteScaleModel.LIMIT_BETWEEN_LOW_MEDIUM_VOTE - 1;

        var result = _voteScaleModel.GetEnumVoteScale(boundary.ToString());

        Assert.AreEqual(EnumVoteScale.Low, result);
    }

    [TestMethod]
    public void GetEnumVoteScale_WithExactLowMediumBoundary_ReturnsMedium()
    {
        // The boundary value itself should be Medium (upper bound is exclusive for Low)
        var result = _voteScaleModel.GetEnumVoteScale(VoteScaleModel.LIMIT_BETWEEN_LOW_MEDIUM_VOTE.ToString());

        Assert.AreEqual(EnumVoteScale.Medium, result);
    }

    [TestMethod]
    public void GetEnumVoteScale_WithJustBelowMediumHighBoundary_ReturnsMedium()
    {
        var boundary = VoteScaleModel.LIMIT_BETWEEN_MEDIUM_HIGH_VOTE - 1;

        var result = _voteScaleModel.GetEnumVoteScale(boundary.ToString());

        Assert.AreEqual(EnumVoteScale.Medium, result);
    }

    [TestMethod]
    public void GetEnumVoteScale_WithExactMediumHighBoundary_ReturnsHigh()
    {
        var result = _voteScaleModel.GetEnumVoteScale(VoteScaleModel.LIMIT_BETWEEN_MEDIUM_HIGH_VOTE.ToString());

        Assert.AreEqual(EnumVoteScale.High, result);
    }

    [TestMethod]
    public void GetEnumVoteScale_WithMaxIntValue_ReturnsHigh()
    {
        var result = _voteScaleModel.GetEnumVoteScale(VoteScaleModel.MAX_VOTE.ToString());

        Assert.AreEqual(EnumVoteScale.High, result);
    }

    [TestMethod]
    public void GetEnumVoteScale_WithWhitespaceOnly_ReturnsEmpty()
    {
        var result = _voteScaleModel.GetEnumVoteScale("   ");

        Assert.AreEqual(EnumVoteScale.Empty, result);
    }

    [TestMethod]
    public void GetEnumVoteScale_WithDecimalLowValue_ReturnsLow()
    {
        var result = _voteScaleModel.GetEnumVoteScale("5.5");

        Assert.AreEqual(EnumVoteScale.Low, result);
    }

    [TestMethod]
    public void GetEnumVoteScale_WithDecimalMediumValue_ReturnsMedium()
    {
        var result = _voteScaleModel.GetEnumVoteScale("25.5");

        Assert.AreEqual(EnumVoteScale.Medium, result);
    }

    [TestMethod]
    public void GetEnumVoteScale_WithDecimalHighValue_ReturnsHigh()
    {
        var result = _voteScaleModel.GetEnumVoteScale("75.5");

        Assert.AreEqual(EnumVoteScale.High, result);
    }
}