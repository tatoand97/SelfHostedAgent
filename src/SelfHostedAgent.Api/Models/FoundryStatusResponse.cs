namespace SelfHostedAgent.Api.Models;

public sealed record FoundryStatusResponse(
    bool Configured,
    bool EndpointConfigured,
    bool DeploymentConfigured,
    string AuthenticationMode,
    string Message);
