using FoundryAgent.Deploy.Services;

namespace FoundryAgent.Deploy.UnitTests.AzureDevOps;

public sealed class AgentIdsFormatterTests
{
    [Fact]
    public void Format_WhenOneAgent_ReturnsNameAndVersion()
    {
        // Arrange
        AgentVersion[] versions = [new("support-agent", "3", "opaque-id")];

        // Act
        var result = AgentIdsFormatter.Format(versions);

        // Assert
        Assert.Equal("support-agent:3", result);
    }

    [Fact]
    public void Format_WhenMultipleAgents_ReturnsCommaSeparatedAgentIds()
    {
        // Arrange
        AgentVersion[] versions =
        [
            new("support-agent", "3", "first-opaque-id"),
            new("collections-agent", "5", "second-opaque-id")
        ];

        // Act
        var result = AgentIdsFormatter.Format(versions);

        // Assert
        Assert.Equal("support-agent:3,collections-agent:5", result);
        Assert.DoesNotContain(" ", result);
    }

    [Fact]
    public void Format_WhenCollectionIsEmpty_ReturnsEmptyString()
    {
        // Arrange
        AgentVersion[] versions = [];

        // Act
        var result = AgentIdsFormatter.Format(versions);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Format_WhenCollectionIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<AgentVersion> versions = null!;

        // Act
        var exception = Record.Exception(() => AgentIdsFormatter.Format(versions));

        // Assert
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Theory]
    [InlineData("support-agent:3", "##vso[task.setvariable variable=AgentIds]support-agent:3")]
    [InlineData("support-agent:3,collections-agent:5", "##vso[task.setvariable variable=AgentIds]support-agent:3,collections-agent:5")]
    public void SetVariableCommand_WhenAgentIdsProvided_ReturnsExpectedCommand(string agentIds, string expected)
    {
        // Arrange / Act
        var result = AgentIdsFormatter.SetVariableCommand(agentIds);

        // Assert
        Assert.Equal(expected, result);
    }
}
