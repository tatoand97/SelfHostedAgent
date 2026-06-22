namespace SelfHostedGovernanceAgent.Api.Models;

public sealed record GovernanceDecision(
    string Decision,
    string RiskLevel,
    bool CanDeploy,
    string[] MissingControls,
    string[] RequiredActions,
    string Summary);
