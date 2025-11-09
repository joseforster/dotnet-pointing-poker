namespace PointingPoker.Tests;

[TestClass]
public class VoteModelTest
{
    [TestMethod]
    public void GetVoteResult_WithHighVoteUserModelList_ReturnHighVoteResult()
    {
        var model1 = new UserModel(string.Empty, string.Empty);
        model1.SetCurrentVote("56");
        var model2 = new UserModel(string.Empty, string.Empty);
        model2.SetCurrentVote("71");
        var model3 = new UserModel(string.Empty, string.Empty);
        model3.SetCurrentVote("67");
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual("64.7", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.High, voteModel.VoteScale);
    }
    
    [TestMethod]
    public void GetVoteResult_WithMediumVoteUserModelList_ReturnMediumVoteResult()
    {
        var model1 = new UserModel(string.Empty, string.Empty);
        model1.SetCurrentVote("17");
        var model2 = new UserModel(string.Empty, string.Empty);
        model2.SetCurrentVote("40");
        var model3 = new UserModel(string.Empty, string.Empty);
        model3.SetCurrentVote("28");
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual("28.3", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Medium, voteModel.VoteScale);
    }
    
    [TestMethod]
    public void GetVoteResult_WithLowVoteUserModelList_ReturnLowVoteResult()
    {
        var model1 = new UserModel(string.Empty, string.Empty);
        model1.SetCurrentVote("7");
        var model2 = new UserModel(string.Empty, string.Empty);
        model2.SetCurrentVote("4");
        var model3 = new UserModel(string.Empty, string.Empty);
        model3.SetCurrentVote("2");
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual("4.3", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Low, voteModel.VoteScale);
    }
    
    [TestMethod]
    public void GetVoteResult_WithOneUndecidedVoteUserModelList_ReturnLowVoteResult()
    {
        var model1 = new UserModel(string.Empty, string.Empty);
        model1.SetCurrentVote("?");
        var model2 = new UserModel(string.Empty, string.Empty);
        model2.SetCurrentVote("4");
        var model3 = new UserModel(string.Empty, string.Empty);
        model3.SetCurrentVote("2");
        
        var userModelList = new List<UserModel>()
        {
            model3,
            model2,
            model1
        };
        
        var voteModel = new VoteModel(userModelList);
        
        Assert.AreEqual("3", voteModel.VoteResult);
        Assert.AreEqual(EnumVoteScale.Low, voteModel.VoteScale);
    }
    
    [TestMethod]
    public void GetVoteResult_WithAllUndecidedVoteUserModelList_ReturnUndecidedVoteResult()
    {
        var model1 = new UserModel(string.Empty, string.Empty);
        model1.SetCurrentVote("?");
        var model2 = new UserModel(string.Empty, string.Empty);
        model2.SetCurrentVote("?");
        var model3 = new UserModel(string.Empty, string.Empty);
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
        var model1 = new UserModel(string.Empty, string.Empty);
        model1.SetCurrentVote(string.Empty);
        var model2 = new UserModel(string.Empty, string.Empty);
        model2.SetCurrentVote(string.Empty);
        var model3 = new UserModel(string.Empty, string.Empty);
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
}