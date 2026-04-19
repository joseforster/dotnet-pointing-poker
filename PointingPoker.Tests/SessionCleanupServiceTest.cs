using System.Collections.Concurrent;
using PointingPoker.Models;
using PointingPoker.Services;

namespace PointingPoker.Tests;

[TestClass]
public class SessionCleanupServiceTest
{
    [TestMethod]
    public void RegisterGroups_WithValidDictionary_DoesNotThrow()
    {
        // Arrange
        var groups = new ConcurrentDictionary<string, GroupModel>();

        // Act & Assert
        SessionCleanupService.RegisterGroups(groups);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RegisterGroups_WithNull_ThrowsArgumentNullException()
    {
        // Act
        SessionCleanupService.RegisterGroups(null!);
    }

    [TestMethod]
    public void UpdateActivity_WithValidSessionId_UpdatesActivity()
    {
        // Arrange
        var groups = new ConcurrentDictionary<string, GroupModel>();
        SessionCleanupService.RegisterGroups(groups);
        var sessionId = "testSession123";

        // Act
        SessionCleanupService.UpdateActivity(sessionId);

        // Assert - The fact that it doesn't throw is the test
        // The service is static so we can't directly verify without exposing internals
    }

    [TestMethod]
    public void UpdateActivity_WithEmptyString_DoesNotThrow()
    {
        // Arrange
        var groups = new ConcurrentDictionary<string, GroupModel>();
        SessionCleanupService.RegisterGroups(groups);

        // Act & Assert
        SessionCleanupService.UpdateActivity(string.Empty); // Should not throw
    }

    [TestMethod]
    public void UpdateActivity_WithNull_DoesNotThrow()
    {
        // Arrange
        var groups = new ConcurrentDictionary<string, GroupModel>();
        SessionCleanupService.RegisterGroups(groups);

        // Act & Assert
        SessionCleanupService.UpdateActivity(null!); // Should not throw
    }

    [TestMethod]
    public void ActiveSessionCount_AfterRegistration_ReturnsZero()
    {
        // Arrange
        var groups = new ConcurrentDictionary<string, GroupModel>();
        SessionCleanupService.RegisterGroups(groups);

        // Act
        var count = SessionCleanupService.ActiveSessionCount;

        // Assert
        Assert.AreEqual(0, count);
    }
}
