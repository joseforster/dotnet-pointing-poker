using PointingPoker.Hubs;

namespace PointingPoker.Tests;

[TestClass]
public class RateLimitFilterTest
{
    [TestMethod]
    public void RemoveConnection_WithValidConnectionId_DoesNotThrow()
    {
        // Arrange
        var connectionId = "testConnection123";

        // Act & Assert
        RateLimitFilter.RemoveConnection(connectionId); // Should not throw even if connection wasn't tracked
    }

    [TestMethod]
    public void RemoveConnection_WithEmptyString_DoesNotThrow()
    {
        // Act & Assert
        RateLimitFilter.RemoveConnection(string.Empty); // Should not throw
    }

    [TestMethod]
    public void RemoveConnection_WithNull_DoesNotThrow()
    {
        // Act & Assert
        RateLimitFilter.RemoveConnection(null!); // Should not throw
    }

    [TestMethod]
    public void RemoveConnection_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var connectionId = "doubleRemoveTest";

        // Act & Assert
        RateLimitFilter.RemoveConnection(connectionId);
        RateLimitFilter.RemoveConnection(connectionId); // Should not throw on second call
    }
}
