using FoundryAgent.Deploy.Agents;
using FoundryAgent.Deploy.Configuration;
using FoundryAgent.Deploy.Services;
using Moq;

namespace FoundryAgent.Deploy.UnitTests.Services;

public sealed class FoundryAgentDeployerTests
{
    [Fact]
    public async Task DeployAsync_WhenAgentsAreNull_ThrowsArgumentNullException()
    {
        var creator = new Mock<IAgentVersionCreator>();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            new FoundryAgentDeployer(creator.Object).DeployAsync(null!));

        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenAgentsAreEmpty_ThrowsInvalidOperationException()
    {
        var creator = new Mock<IAgentVersionCreator>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object).DeployAsync([]));

        Assert.Contains("at least one", exception.Message);
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenAgentIsInvalid_ThrowsBeforeRemoteOperations()
    {
        var creator = new Mock<IAgentVersionCreator>();
        var agents = new[] { new AgentSpecification("", "Instructions") };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(agents));

        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenAgentNamesAreDuplicated_ThrowsBeforeRemoteOperations()
    {
        var creator = new Mock<IAgentVersionCreator>();
        var agents = new[]
        {
            new AgentSpecification("support-agent", "First"),
            new AgentSpecification("SUPPORT-AGENT", "Second")
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(agents));

        Assert.Contains("Duplicate agent name", exception.Message);
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenAnyAgentIsInvalid_ValidatesAllBeforeRemoteOperations()
    {
        var creator = new Mock<IAgentVersionCreator>();
        var agents = new[]
        {
            new AgentSpecification("valid-agent", "First"),
            new AgentSpecification("invalid agent", "Second")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(agents));

        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenOneAgentUsesGlobalModel_CreatesItOnceWithExpectedRequest()
    {
        var creator = new Mock<IAgentVersionCreator>();
        AgentVersionCreationRequest? request = null;
        creator.Setup(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AgentVersionCreationRequest, CancellationToken>((value, _) => request = value)
            .ReturnsAsync(new AgentVersion("support-agent", "3", "support-agent:3"));

        var result = await new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(
            [new AgentSpecification("support-agent", "Provide support.")]);

        creator.Verify(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(new AgentVersionCreationRequest("support-agent", "Provide support.", "gpt-4o"), request);
        Assert.Equal([new AgentVersion("support-agent", "3", "support-agent:3")], result);
    }

    [Fact]
    public async Task DeployAsync_WhenAgentOverridesModel_UsesAgentModel()
    {
        var creator = new Mock<IAgentVersionCreator>();
        AgentVersionCreationRequest? request = null;
        creator.Setup(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AgentVersionCreationRequest, CancellationToken>((value, _) => request = value)
            .ReturnsAsync(new AgentVersion("support-agent", "4", "support-agent:4"));

        await new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(
            [new AgentSpecification("support-agent", "Provide support.", "gpt-4o-mini")]);

        Assert.Equal("gpt-4o-mini", request?.ModelDeploymentName);
        Assert.NotEqual(FoundryConfiguration.ModelDeploymentName, request?.ModelDeploymentName);
    }

    [Fact]
    public async Task DeployAsync_WhenMultipleAgentsAreValid_CreatesAllInOrder()
    {
        var creator = new Mock<IAgentVersionCreator>();
        var requests = new List<AgentVersionCreationRequest>();
        creator.Setup(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AgentVersionCreationRequest, CancellationToken>((value, _) => requests.Add(value))
            .Returns((AgentVersionCreationRequest value, CancellationToken _) =>
                Task.FromResult(new AgentVersion(value.Name, value.Name == "first-agent" ? "3" : "5", value.Name)));

        var result = await new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(
        [
            new AgentSpecification("first-agent", "First instructions"),
            new AgentSpecification("second-agent", "Second instructions")
        ]);

        creator.Verify(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        Assert.Equal(["first-agent", "second-agent"], requests.Select(request => request.Name));
        Assert.Equal(["first-agent:3", "second-agent:5"], result.Select(version => $"{version.Name}:{version.Version}"));
    }

    [Fact]
    public async Task DeployAsync_WhenRemoteCreationFails_PropagatesException()
    {
        var creator = new Mock<IAgentVersionCreator>();
        var expected = new InvalidOperationException("remote failure");
        creator.Setup(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(
                [new AgentSpecification("support-agent", "Provide support.")]));

        Assert.Same(expected, actual);
    }
}
