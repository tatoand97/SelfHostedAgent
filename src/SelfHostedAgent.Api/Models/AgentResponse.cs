namespace SelfHostedAgent.Api.Models;

public sealed record AgentResponse(
    string Answer,
    string AgentName,
    string ModelDeploymentName,
    string? CorrelationId,
    bool UsedBusinessContext,
    string AuthenticationMode);
