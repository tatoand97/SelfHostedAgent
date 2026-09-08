using FoundryAgent.Deploy.Agents;

namespace FoundryAgent.Deploy.UnitTests.Agents;

public sealed class AgentSpecificationTests
{
    [Fact]
    public void Validate_WhenSpecificationIsValid_DoesNotThrow()
    {
        new AgentSpecification("support-agent", "Provide support.").Validate("gpt-4o");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenNameIsEmpty_ThrowsInvalidOperationException(string? name)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new AgentSpecification(name!, "Provide support.").Validate());

        Assert.Contains("Name", exception.Message);
    }

    [Theory]
    [InlineData("support agent")]
    [InlineData("support:agent")]
    [InlineData("support,agent")]
    [InlineData("support%agent")]
    [InlineData("support\u0001agent")]
    public void Validate_WhenNameContainsForbiddenCharacter_ThrowsInvalidOperationException(string name)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new AgentSpecification(name, "Provide support.").Validate());

        Assert.Contains("cannot contain", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenInstructionsAreEmpty_ThrowsInvalidOperationException(string? instructions)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new AgentSpecification("support-agent", instructions!).Validate());

        Assert.Contains("Instructions", exception.Message);
    }

    [Fact]
    public void Validate_WhenModelDeploymentIsSpecificAndValid_DoesNotThrow()
    {
        new AgentSpecification("support-agent", "Provide support.", "gpt-4o-mini").Validate();
    }

    [Fact]
    public void Validate_WhenModelDeploymentIsNull_UsesGlobalModelAndDoesNotThrow()
    {
        new AgentSpecification("support-agent", "Provide support.", null).Validate("gpt-4o");
    }

    [Fact]
    public void Validate_WhenModelDeploymentIsPlaceholder_ThrowsInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new AgentSpecification("support-agent", "Provide support.", "REPLACE_WITH_MODEL").Validate());

        Assert.Contains("model deployment", exception.Message);
    }
}
