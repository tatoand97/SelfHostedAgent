using FoundryAgent.Deploy.Agents;

namespace FoundryAgent.Deploy.UnitTests.Agents;

public sealed class AgentSpecificationTests
{
    [Fact]
    public void Validate_WhenSpecificationIsValid_DoesNotThrow()
    {
        // Arrange
        var agent = new AgentSpecification("support-agent", "Provide support.");

        // Act
        var exception = Record.Exception(() => agent.Validate("global-model"));

        // Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenNameIsEmpty_ThrowsInvalidOperationException(string? name)
    {
        // Arrange
        var agent = new AgentSpecification(name!, "Provide support.");

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => agent.Validate("global-model"));

        // Assert
        Assert.Contains("Name", exception.Message);
    }

    [Theory]
    [InlineData("support agent")]
    [InlineData("support:agent")]
    [InlineData("support,agent")]
    [InlineData("support%agent")]
    [InlineData("support\u0001agent")]
    [InlineData("support\nagent")]
    [InlineData("support\ragent")]
    [InlineData("support\tagent")]
    public void Validate_WhenNameContainsForbiddenCharacter_ThrowsInvalidOperationException(string name)
    {
        // Arrange
        var agent = new AgentSpecification(name, "Provide support.");

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => agent.Validate("global-model"));

        // Assert
        Assert.Contains("cannot contain", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenInstructionsAreEmpty_ThrowsInvalidOperationException(string? instructions)
    {
        // Arrange
        var agent = new AgentSpecification("support-agent", instructions!);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => agent.Validate("global-model"));

        // Assert
        Assert.Contains("Instructions", exception.Message);
    }

    [Fact]
    public void Validate_WhenModelDeploymentIsSpecificAndValid_IgnoresInvalidGlobalModel()
    {
        // Arrange
        var agent = new AgentSpecification("support-agent", "Provide support.", "specific-model");

        // Act
        var exception = Record.Exception(() => agent.Validate(""));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenModelDeploymentIsNull_UsesGlobalModel()
    {
        // Arrange
        var agent = new AgentSpecification("support-agent", "Provide support.", null);

        // Act
        var exception = Record.Exception(() => agent.Validate("global-model"));

        // Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("REPLACE_WITH_MODEL")]
    public void Validate_WhenSpecificModelIsInvalid_ThrowsInsteadOfFallingBack(string model)
    {
        // Arrange
        var agent = new AgentSpecification("support-agent", "Provide support.", model);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => agent.Validate("valid-global-model"));

        // Assert
        Assert.Contains("model deployment", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("REPLACE_WITH_MODEL")]
    public void Validate_WhenFallbackModelIsInvalid_ThrowsInvalidOperationException(string model)
    {
        // Arrange
        var agent = new AgentSpecification("support-agent", "Provide support.");

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => agent.Validate(model));

        // Assert
        Assert.Contains("model deployment", exception.Message);
    }
}
