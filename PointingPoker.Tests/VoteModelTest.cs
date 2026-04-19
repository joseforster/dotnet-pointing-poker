using PointingPoker.Enums;
using PointingPoker.Models;

namespace PointingPoker.Tests;

[TestClass]
public class VoteModelTest
{
    [TestMethod]
    public void GetVoteResult_WithHighVoteUserModelList_ReturnHighVoteResult()
    {
        var model1 = CreateEmptyUserModel();
        model1.SetCurrentVote("56");
        var model2 = CreateEmptyUserModel();
        model2.SetCurrentVote("71");
        var model3 = CreateEmptyUserModel();
        model3.SetCurrentVote("67");
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual("64.5", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.High, voteModel.VoteScale);
    }
    
    [TestMethod]
    public void GetVoteResult_WithMediumVoteUserModelList_ReturnMediumVoteResult()
    {
        var model1 = CreateEmptyUserModel();
        model1.SetCurrentVote("17");
        var model2 = CreateEmptyUserModel();
        model2.SetCurrentVote("40");
        var model3 = CreateEmptyUserModel();
        model3.SetCurrentVote("28");
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual("28.5", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Medium, voteModel.VoteScale);
    }
    
    [TestMethod]
    public void GetVoteResult_WithLowVoteUserModelList_ReturnLowVoteResult()
    {
        var model1 = CreateEmptyUserModel();
        model1.SetCurrentVote("7");
        var model2 = CreateEmptyUserModel();
        model2.SetCurrentVote("4");
        var model3 = CreateEmptyUserModel();
        model3.SetCurrentVote("2");
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual("4.5", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Low, voteModel.VoteScale);
    }
    
    [TestMethod]
    public void GetVoteResult_WithOneUndecidedVoteUserModelList_ReturnLowVoteResult()
    {
        var model1 = CreateEmptyUserModel();
        model1.SetCurrentVote("?");
        var model2 = CreateEmptyUserModel();
        model2.SetCurrentVote("4");
        var model3 = CreateEmptyUserModel();
        model3.SetCurrentVote("2");
        var model4 = CreateEmptyUserModel();
        model4.SetCurrentVote(string.Empty);
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1,
            model4
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual("3", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Low, voteModel.VoteScale);
    }
    
    [TestMethod]
    public void GetVoteResult_WithAllUndecidedVoteUserModelList_ReturnUndecidedVoteResult()
    {
        var model1 = CreateEmptyUserModel();
        model1.SetCurrentVote("?");
        var model2 = CreateEmptyUserModel();
        model2.SetCurrentVote("?");
        var model3 = CreateEmptyUserModel();
        model3.SetCurrentVote("?");
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual("?", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Undecided, voteModel.VoteScale);
    }
    
    [TestMethod]
    public void GetVoteResult_WithAllEmptyVoteUserModelList_ReturnEmptyVoteResult()
    {
        var model1 = CreateEmptyUserModel();
        model1.SetCurrentVote(string.Empty);
        var model2 = CreateEmptyUserModel();
        model2.SetCurrentVote(string.Empty);
        var model3 = CreateEmptyUserModel();
        model3.SetCurrentVote(string.Empty);
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual(string.Empty, voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Empty, voteModel.VoteScale);
    }

    [TestMethod]
    public void GetVoteResult_WithSingleVote_ReturnsThatVote()
    {
        var model = CreateEmptyUserModel();
        model.SetCurrentVote("5");

        var userModelList = new List<UserModel>() { model };
        var voteModel = new VoteModel(userModelList);

        Assert.AreEqual("5", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Low, voteModel.VoteScale);
    }

    [TestMethod]
    public void GetVoteResult_WithDecimalVotes_ReturnsRoundedHalfPoint()
    {
        // Tests the rounding logic: Math.Round(voteValue * 2, MidpointRounding.AwayFromZero) / 2
        var model1 = CreateEmptyUserModel();
        model1.SetCurrentVote("1.1");
        var model2 = CreateEmptyUserModel();
        model2.SetCurrentVote("1.4"); // Average = 1.25, rounds to 1.5

        var userModelList = new List<UserModel>() { model1, model2 };
        var voteModel = new VoteModel(userModelList);

        Assert.AreEqual("1.5", voteModel.VoteResult);
    }

    [TestMethod]
    public void GetVoteResult_WithMixedNumericAndEmpty_ReturnsAverageOfNumericOnly()
    {
        var model1 = CreateEmptyUserModel();
        model1.SetCurrentVote("10");
        var model2 = CreateEmptyUserModel();
        model2.SetCurrentVote("20");
        var model3 = CreateEmptyUserModel();
        model3.SetCurrentVote(string.Empty); // HasVoted = false, won't be counted

        var userModelList = new List<UserModel>() { model1, model2, model3 };
        var voteModel = new VoteModel(userModelList);

        Assert.AreEqual("15", voteModel.VoteResult); // (10+20)/2 = 15
        Assert.AreEqual(EnumVoteScale.Medium, voteModel.VoteScale);
    }

    [TestMethod]
    public void GetVoteResult_WithManyUsers_CalculatesCorrectly()
    {
        var users = new List<UserModel>();
        for (int i = 0; i < 10; i++)
        {
            var user = CreateEmptyUserModel();
            user.SetCurrentVote(i.ToString());
            users.Add(user);
        }

        var voteModel = new VoteModel(users);

        // Average of 0+1+2+3+4+5+6+7+8+9 = 45, 45/10 = 4.5
        Assert.AreEqual("4.5", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Low, voteModel.VoteScale);
    }

    [TestMethod]
    public void GetVoteResult_WithLargeNumbers_HandlesCorrectly()
    {
        var model1 = CreateEmptyUserModel();
        model1.SetCurrentVote(VoteScaleModel.MAX_VOTE.ToString());
        var model2 = CreateEmptyUserModel();
        model2.SetCurrentVote(VoteScaleModel.MAX_VOTE.ToString());

        var userModelList = new List<UserModel>() { model1, model2 };
        var voteModel = new VoteModel(userModelList);

        Assert.AreEqual("100", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.High, voteModel.VoteScale);
    }

    private UserModel CreateEmptyUserModel()
    {
        return new UserModel(string.Empty, string.Empty, string.Empty, Guid.NewGuid().ToString());
    }
}