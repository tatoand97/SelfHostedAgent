using FoundryAgent.Deploy.Services;

namespace FoundryAgent.Deploy.UnitTests.AzureDevOps;

public sealed class AgentIdsFormatterTests
{
    [Fact]
    public void Format_WhenOneAgent_ReturnsNameAndVersion()
    {
        var result = AgentIdsFormatter.Format(
            [new AgentVersion("support-agent", "3", "support-agent:3")]);

        Assert.Equal("support-agent:3", result);
    }

    [Fact]
    public void Format_WhenMultipleAgents_ReturnsCommaSeparatedAgentIds()
    {
        var result = AgentIdsFormatter.Format(
        [
            new AgentVersion("support-agent", "3", "support-agent:3"),
            new AgentVersion("collections-agent", "5", "collections-agent:5")
        ]);

        Assert.Equal("support-agent:3,collections-agent:5", result);
    }

    [Fact]
    public void Format_WhenCollectionIsEmpty_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, AgentIdsFormatter.Format([]));
    }

    [Fact]
    public void SetVariableCommand_WhenOneAgent_ReturnsExpectedAzureDevOpsCommand()
    {
        var result = AgentIdsFormatter.SetVariableCommand("support-agent:3");

        Assert.Equal("##vso[task.setvariable variable=AgentIds]support-agent:3", result);
    }
}
