namespace SelfHostedAgent.Api.Models;

public sealed record AgentRequest(
    string Question,
    string? CorrelationId);
