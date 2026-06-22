namespace SelfHostedAgent.Api.Models;

public sealed record FoundryStatusResponse(
    bool Configured,
    bool ProjectEndpointConfigured,
    bool ModelDeploymentConfigured,
    string Provider,
    string AuthenticationMode,
    string Message);
