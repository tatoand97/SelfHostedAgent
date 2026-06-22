using SelfHostedGovernanceAgent.Api.Models;

namespace SelfHostedGovernanceAgent.Api.Services;

public interface IGovernanceEvaluationService
{
    GovernanceDecision Evaluate(AgentDeploymentRequest request);
}
