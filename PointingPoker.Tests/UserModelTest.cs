using PointingPoker.Enums;
using PointingPoker.Models;

namespace PointingPoker.Tests;

[TestClass]
public class UserModelTest
{
    private const string TestConnectionId = "conn123";
    private const string TestUsername = "TestUser";
    private const string TestSessionId = "session456";
    private const string TestGuid = "guid789";

    [TestMethod]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Act
        var user = new UserModel(TestConnectionId, TestUsername, TestSessionId, TestGuid);

        // Assert
        Assert.AreEqual(TestConnectionId, user.ConnectionId);
        Assert.AreEqual(TestUsername, user.Username);
        Assert.AreEqual(TestSessionId, user.SessionId);
        Assert.AreEqual(TestGuid, user.Guid);
        Assert.AreEqual(string.Empty, user.CurrentVote);
        Assert.IsFalse(user.HasVoted);
    }

    [TestMethod]
    public void SetCurrentVote_WithNumericVote_SetsVoteAndScale()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        user.SetCurrentVote("80");

        // Assert
        Assert.AreEqual("80", user.CurrentVote);
        Assert.IsTrue(user.HasVoted);
        Assert.IsTrue(user.IsVoteANumber);
        Assert.AreEqual(EnumVoteScale.High, user.VoteScale);
    }

    [TestMethod]
    public void SetCurrentVote_WithUndecidedVote_SetsVoteCorrectly()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        user.SetCurrentVote("?");

        // Assert
        Assert.AreEqual("?", user.CurrentVote);
        Assert.IsTrue(user.HasVoted);
        Assert.IsFalse(user.IsVoteANumber);
        Assert.AreEqual(EnumVoteScale.Undecided, user.VoteScale);
    }

    [TestMethod]
    public void SetCurrentVote_WithEmptyVote_ResetsVote()
    {
        // Arrange
        var user = CreateDefaultUser();
        user.SetCurrentVote("5");

        // Act
        user.SetCurrentVote(string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, user.CurrentVote);
        Assert.IsFalse(user.HasVoted);
        Assert.IsFalse(user.IsVoteANumber);
        Assert.AreEqual(EnumVoteScale.Empty, user.VoteScale);
    }

    [TestMethod]
    public void SetCurrentVote_WithDecimalVote_SetsVoteCorrectly()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        user.SetCurrentVote("3.5");

        // Assert
        Assert.AreEqual("3.5", user.CurrentVote);
        Assert.IsTrue(user.HasVoted);
        Assert.IsTrue(user.IsVoteANumber);
        Assert.AreEqual(EnumVoteScale.Low, user.VoteScale);
    }

    [TestMethod]
    public void IsVoteANumber_WithNonNumericVote_ReturnsFalse()
    {
        // Arrange
        var user = CreateDefaultUser();
        user.SetCurrentVote("coffee");

        // Assert
        Assert.IsFalse(user.IsVoteANumber);
        Assert.AreEqual(EnumVoteScale.Empty, user.VoteScale);
    }

    [TestMethod]
    public void HasVoted_AfterSettingAndClearingVote_ReturnsCorrectState()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act & Assert - Initially false
        Assert.IsFalse(user.HasVoted);

        // Act - Set vote
        user.SetCurrentVote("5");
        Assert.IsTrue(user.HasVoted);

        // Act - Clear vote
        user.SetCurrentVote(string.Empty);
        Assert.IsFalse(user.HasVoted);
    }

    [TestMethod]
    public void SetConnectionId_UpdatesConnectionId()
    {
        // Arrange
        var user = CreateDefaultUser();
        var newConnectionId = "newConn456";

        // Act
        user.SetConnectionId(newConnectionId);

        // Assert
        Assert.AreEqual(newConnectionId, user.ConnectionId);
    }

    [TestMethod]
    public void SetConnectionId_DoesNotAffectOtherProperties()
    {
        // Arrange
        var user = CreateDefaultUser();
        user.SetCurrentVote("8");

        // Act
        user.SetConnectionId("newConn");

        // Assert
        Assert.AreEqual(TestUsername, user.Username);
        Assert.AreEqual(TestSessionId, user.SessionId);
        Assert.AreEqual(TestGuid, user.Guid);
        Assert.AreEqual("8", user.CurrentVote);
        Assert.IsTrue(user.HasVoted);
    }

    [TestMethod]
    public void VoteScale_UpdatesCorrectlyWithMultipleVoteChanges()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act & Assert
        user.SetCurrentVote("2");
        Assert.AreEqual(EnumVoteScale.Low, user.VoteScale);

        user.SetCurrentVote("20");
        Assert.AreEqual(EnumVoteScale.Medium, user.VoteScale);

        user.SetCurrentVote("100");
        Assert.AreEqual(EnumVoteScale.High, user.VoteScale);

        user.SetCurrentVote("?");
        Assert.AreEqual(EnumVoteScale.Undecided, user.VoteScale);

        user.SetCurrentVote(string.Empty);
        Assert.AreEqual(EnumVoteScale.Empty, user.VoteScale);
    }

    [TestMethod]
    public void Constructor_WithNullUsername_HandlesGracefully()
    {
        // Act
        var user = new UserModel(TestConnectionId, null!, TestSessionId, TestGuid);

        // Assert
        Assert.IsNull(user.Username);
    }

    private UserModel CreateDefaultUser()
    {
        return new UserModel(TestConnectionId, TestUsername, TestSessionId, TestGuid);
    }
}
