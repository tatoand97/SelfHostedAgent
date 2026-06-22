namespace SelfHostedGovernanceAgent.Api.Models;

public sealed record GovernanceChatResponse(
    string Response,
    string ModelDeploymentName,
    string AuthenticatedWith,
    string Summary);
