using FoundryAgent.Deploy.Configuration;

namespace FoundryAgent.Deploy.UnitTests.Configuration;

public sealed class FoundryConfigurationTests
{
    [Fact]
    public void Validate_WhenEndpointAndModelAreValid_DoesNotThrow()
    {
        FoundryConfiguration.Validate("https://project.services.ai.azure.com/api/projects/demo", "gpt-4o");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a uri")]
    [InlineData("http://project.services.ai.azure.com/api/projects/demo")]
    public void Validate_WhenEndpointIsInvalid_ThrowsInvalidOperationException(string? endpoint)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            FoundryConfiguration.Validate(endpoint!, "gpt-4o"));

        Assert.Contains("ProjectEndpoint", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenModelIsMissing_ThrowsInvalidOperationException(string? model)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            FoundryConfiguration.Validate("https://project.services.ai.azure.com", model!));

        Assert.Contains("model deployment", exception.Message);
    }

    [Fact]
    public void Validate_WhenEndpointContainsCredentials_ThrowsInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            FoundryConfiguration.Validate("https://user:secret@project.services.ai.azure.com", "gpt-4o"));

        Assert.Contains("without credentials", exception.Message);
    }
}
