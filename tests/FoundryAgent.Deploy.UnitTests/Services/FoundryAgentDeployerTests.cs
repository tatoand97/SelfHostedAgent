using FoundryAgent.Deploy.Agents;
using FoundryAgent.Deploy.Services;
using Moq;

namespace FoundryAgent.Deploy.UnitTests.Services;

public sealed class FoundryAgentDeployerTests
{
    [Fact]
    public async Task DeployAsync_WhenAgentsAreNull_ThrowsArgumentNullException()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);

        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            new FoundryAgentDeployer(creator.Object).DeployAsync(null!));

        // Assert
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenAgentsAreEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object).DeployAsync([]));

        // Assert
        Assert.Contains("at least one", exception.Message);
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenAgentIsInvalid_ThrowsBeforeRemoteOperations()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var agents = new[] { new AgentSpecification("", "Instructions") };

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(agents));

        // Assert
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenAgentNamesAreDuplicated_ThrowsBeforeRemoteOperations()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var agents = new[]
        {
            new AgentSpecification("support-agent", "First"),
            new AgentSpecification("SUPPORT-AGENT", "Second")
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(agents));

        // Assert
        Assert.Contains("Duplicate agent name", exception.Message);
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenAnyAgentIsInvalid_ValidatesAllBeforeRemoteOperations()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var agents = new[]
        {
            new AgentSpecification("valid-agent", "First"),
            new AgentSpecification("invalid agent", "Second")
        };

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(agents));

        // Assert
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenOneAgentUsesGlobalModel_CreatesItOnceWithExpectedRequest()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        AgentVersionCreationRequest? request = null;
        creator.Setup(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AgentVersionCreationRequest, CancellationToken>((value, _) => request = value)
            .ReturnsAsync(new AgentVersion("support-agent", "3", "support-agent:3"));

        // Act
        var result = await new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(
            [new AgentSpecification("support-agent", "Provide support.")]);

        // Assert
        creator.Verify(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(new AgentVersionCreationRequest("support-agent", "Provide support.", "gpt-4o"), request);
        Assert.Equal([new AgentVersion("support-agent", "3", "support-agent:3")], result);
    }

    [Fact]
    public async Task DeployAsync_WhenAgentOverridesModel_UsesAgentModel()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        AgentVersionCreationRequest? request = null;
        creator.Setup(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AgentVersionCreationRequest, CancellationToken>((value, _) => request = value)
            .ReturnsAsync(new AgentVersion("support-agent", "4", "support-agent:4"));

        // Act
        await new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(
            [new AgentSpecification("support-agent", "Provide support.", "gpt-4o-mini")]);

        // Assert
        Assert.Equal("gpt-4o-mini", request?.ModelDeploymentName);
        creator.Verify(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeployAsync_WhenMultipleAgentsAreValid_CreatesAllInOrder()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var requests = new List<AgentVersionCreationRequest>();
        creator.Setup(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AgentVersionCreationRequest, CancellationToken>((value, _) => requests.Add(value))
            .Returns((AgentVersionCreationRequest value, CancellationToken _) =>
                Task.FromResult(new AgentVersion(value.Name, value.Name == "first-agent" ? "3" : "5", value.Name)));

        // Act
        var result = await new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(
        [
            new AgentSpecification("first-agent", "First instructions"),
            new AgentSpecification("second-agent", "Second instructions")
        ]);

        // Assert
        creator.Verify(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        Assert.Equal(["first-agent", "second-agent"], requests.Select(request => request.Name));
        Assert.Equal(["first-agent:3", "second-agent:5"], result.Select(version => $"{version.Name}:{version.Version}"));
    }

    [Fact]
    public async Task DeployAsync_WhenRemoteCreationFails_PropagatesException()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var expected = new InvalidOperationException("remote failure");
        creator.Setup(value => value.CreateAsync(It.IsAny<AgentVersionCreationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);

        // Act
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FoundryAgentDeployer(creator.Object, "gpt-4o").DeployAsync(
                [new AgentSpecification("support-agent", "Provide support.")]));

        // Assert
        Assert.Same(expected, actual);
    }
    [Fact]
    public async Task DeployAsync_WhenLaterAgentIsNull_ThrowsBeforeRemoteOperations()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var deployer = new FoundryAgentDeployer(creator.Object, "global-model");
        AgentSpecification[] agents = [new("valid-agent", "Instructions"), null!];

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => deployer.DeployAsync(agents));

        // Assert
        Assert.Equal("agent", exception.ParamName);
        creator.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("", "Instructions", null)]
    [InlineData("valid-agent", "", null)]
    [InlineData("valid-agent", " ", null)]
    [InlineData("valid-agent", "Instructions", "")]
    [InlineData("valid-agent", "Instructions", "REPLACE_WITH_MODEL")]
    public async Task DeployAsync_WhenLaterSpecificationIsInvalid_MakesNoRemoteCalls(
        string name, string instructions, string? model)
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var deployer = new FoundryAgentDeployer(creator.Object, "global-model");
        AgentSpecification[] agents =
        [
            new("first-agent", "Valid instructions"),
            new(name, instructions, model)
        ];

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => deployer.DeployAsync(agents));

        // Assert
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenRemoteReturnsDifferentIdentity_PreservesReturnedNameVersionAndId()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var expected = new AgentVersion("returned-agent", "42", "opaque-server-id");
        var request = new AgentVersionCreationRequest("requested-agent", "Instructions", "global-model");
        creator.Setup(value => value.CreateAsync(request, default)).ReturnsAsync(expected);
        var deployer = new FoundryAgentDeployer(creator.Object, "global-model");

        // Act
        var versions = await deployer.DeployAsync([new("requested-agent", "Instructions")]);

        // Assert
        Assert.Same(expected, Assert.Single(versions));
        creator.Verify(value => value.CreateAsync(request, default), Times.Once);
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenSecondCreationFails_PropagatesExceptionWithoutReturningPartialSuccess()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var expected = new HttpRequestException("Simulated Foundry failure; no HTTP request was sent.");
        creator.Setup(value => value.CreateAsync(
            new AgentVersionCreationRequest("first-agent", "First", "global-model"), default))
            .ReturnsAsync(new AgentVersion("first-agent", "3", "first-id"));
        creator.Setup(value => value.CreateAsync(
            new AgentVersionCreationRequest("second-agent", "Second", "global-model"), default))
            .ThrowsAsync(expected);
        var deployer = new FoundryAgentDeployer(creator.Object, "global-model");
        IReadOnlyList<AgentVersion>? result = null;

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            result = await deployer.DeployAsync(
            [
                new("first-agent", "First"),
                new("second-agent", "Second"),
                new("third-agent", "Must not be sent")
            ]);
        });

        // Assert
        Assert.Same(expected, exception);
        Assert.Null(result);
        creator.Verify(value => value.CreateAsync(
            new AgentVersionCreationRequest("first-agent", "First", "global-model"), default), Times.Once);
        creator.Verify(value => value.CreateAsync(
            new AgentVersionCreationRequest("second-agent", "Second", "global-model"), default), Times.Once);
        creator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeployAsync_WhenModelsAreMixed_UsesGlobalAndSpecificDeployments()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var globalRequest = new AgentVersionCreationRequest("first-agent", "First", "global-model");
        var overrideRequest = new AgentVersionCreationRequest("second-agent", "Second", "specific-model");
        creator.Setup(value => value.CreateAsync(globalRequest, default))
            .ReturnsAsync(new AgentVersion("first-agent", "3", "first-id"));
        creator.Setup(value => value.CreateAsync(overrideRequest, default))
            .ReturnsAsync(new AgentVersion("second-agent", "5", "second-id"));
        var deployer = new FoundryAgentDeployer(creator.Object, "global-model");

        // Act
        var versions = await deployer.DeployAsync(
        [
            new("first-agent", "First"),
            new("second-agent", "Second", "specific-model")
        ]);

        // Assert
        Assert.Equal(2, versions.Count);
        creator.Verify(value => value.CreateAsync(globalRequest, default), Times.Once);
        creator.Verify(value => value.CreateAsync(overrideRequest, default), Times.Once);
        creator.VerifyNoOtherCalls();
    }
}
