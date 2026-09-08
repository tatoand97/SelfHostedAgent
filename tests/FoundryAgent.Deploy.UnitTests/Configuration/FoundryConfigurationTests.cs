using FoundryAgent.Deploy.Agents;
using FoundryAgent.Deploy.Services;
using Moq;
using FoundryAgent.Deploy.Configuration;

namespace FoundryAgent.Deploy.UnitTests.Configuration;

// Environment variables are process-wide. No other collection may run alongside this one.
[CollectionDefinition("Environment configuration", DisableParallelization = true)]
public sealed class EnvironmentConfigurationCollection;

[Collection("Environment configuration")]
public sealed class FoundryConfigurationTests : IDisposable
{
    private const string ValidEndpoint = "https://project.services.ai.azure.com/api/projects/demo";
    private readonly string? originalEndpoint = Environment.GetEnvironmentVariable("AzureAIProjectEndpoint");
    private readonly string? originalModel = Environment.GetEnvironmentVariable("DeploymentName");

    public FoundryConfigurationTests()
    {
        Environment.SetEnvironmentVariable("AzureAIProjectEndpoint", ValidEndpoint);
        Environment.SetEnvironmentVariable("DeploymentName", "global-model");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("AzureAIProjectEndpoint", originalEndpoint);
        Environment.SetEnvironmentVariable("DeploymentName", originalModel);
    }

    [Fact]
    public void Validate_WhenEnvironmentIsValid_UsesEndpointAndDeploymentName()
    {
        // Arrange: known values are set for each test, regardless of the host environment.

        // Act
        var exception = Record.Exception(FoundryConfiguration.Validate);

        // Assert
        Assert.Null(exception);
        Assert.Equal(ValidEndpoint, FoundryConfiguration.ProjectEndpoint);
        Assert.Equal("global-model", FoundryConfiguration.ModelDeploymentName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a uri")]
    [InlineData("http://project.services.ai.azure.com/api/projects/demo")]
    [InlineData("REPLACE_WITH_PROJECT_ENDPOINT")]
    [InlineData("https://user:password@project.services.ai.azure.com")]
    public void Validate_WhenAzureAIProjectEndpointIsInvalid_ThrowsInvalidOperationException(string? endpoint)
    {
        // Arrange
        Environment.SetEnvironmentVariable("AzureAIProjectEndpoint", endpoint);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(FoundryConfiguration.Validate);

        // Assert
        Assert.Contains("AzureAIProjectEndpoint", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("REPLACE_WITH_MODEL")]
    public void Validate_WhenDeploymentNameIsInvalid_ThrowsInvalidOperationException(string? model)
    {
        // Arrange
        Environment.SetEnvironmentVariable("DeploymentName", model);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(FoundryConfiguration.Validate);

        // Assert
        Assert.Contains("DeploymentName", exception.Message);
    }

    [Fact]
    public void Validate_WhenExplicitValuesAreValid_DoesNotReadInvalidEnvironment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AzureAIProjectEndpoint", null);
        Environment.SetEnvironmentVariable("DeploymentName", null);

        // Act
        var exception = Record.Exception(() => FoundryConfiguration.Validate(ValidEndpoint, "explicit-model"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenAgentModelIsNull_FallsBackToEnvironmentDeploymentName()
    {
        // Arrange
        var agent = new AgentSpecification("support-agent", "Instructions");

        // Act
        var exception = Record.Exception(() => agent.Validate());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task DeployAsync_WhenNoGlobalModelIsPassed_UsesEnvironmentDeploymentName()
    {
        // Arrange
        var creator = new Mock<IAgentVersionCreator>(MockBehavior.Strict);
        var request = new AgentVersionCreationRequest(
            "support-agent", "Instructions", "global-model");
        creator.Setup(value => value.CreateAsync(request, default))
            .ReturnsAsync(new AgentVersion("support-agent", "3", "opaque-id"));
        var deployer = new FoundryAgentDeployer(creator.Object);

        // Act
        var versions = await deployer.DeployAsync([new("support-agent", "Instructions")]);

        // Assert
        Assert.Single(versions);
        creator.Verify(value => value.CreateAsync(request, default), Times.Once);
        creator.VerifyNoOtherCalls();
    }
}
