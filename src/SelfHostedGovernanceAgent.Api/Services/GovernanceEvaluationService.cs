using SelfHostedGovernanceAgent.Api.Models;

namespace SelfHostedGovernanceAgent.Api.Services;

public sealed class GovernanceEvaluationService : IGovernanceEvaluationService
{
    public GovernanceDecision Evaluate(AgentDeploymentRequest request)
    {
        var missingControls = new List<string>();
        var requiredActions = new List<string>();
        var isProd = string.Equals(request.TargetEnvironment, "prod", StringComparison.OrdinalIgnoreCase);
        var blocked = false;
        var needsReview = false;

        void Block(string missingControl, string requiredAction)
        {
            blocked = true;
            missingControls.Add(missingControl);
            requiredActions.Add(requiredAction);
        }

        void Review(string missingControl, string requiredAction)
        {
            needsReview = true;
            missingControls.Add(missingControl);
            requiredActions.Add(requiredAction);
        }

        if (string.IsNullOrWhiteSpace(request.AgentName))
        {
            Block("agentName", "Provide a non-empty agent name before deployment.");
        }

        if (isProd && !request.HasSast)
        {
            Block("SAST", "Run SAST successfully before deploying to production.");
        }

        if (isProd && !request.HasUnitTests)
        {
            Block("UnitTests", "Add and pass unit tests before deploying to production.");
        }

        if (isProd && !request.HasAiEvaluation)
        {
            Block("AIEvaluation", "Run AI evaluation before deploying an agent to production.");
        }

        if (request.UsesSensitiveData && !request.UsesManagedIdentity)
        {
            Block("ManagedIdentity", "Use managed identity for agents that process sensitive data.");
        }

        if (request.UsesInternalData && !request.UsesManagedIdentity)
        {
            Block("ManagedIdentity", "Use managed identity for agents that access internal data.");
        }

        if (request.UsesExternalApis && !request.HasDependencyScan)
        {
            Review("DependencyScan", "Run dependency scanning before integrating external APIs.");
        }

        if (!request.HasSecretScan)
        {
            Review("SecretScan", "Run secret scanning before deployment.");
        }

        var decision = blocked ? "Blocked" : needsReview ? "NeedsReview" : "Approved";
        var riskLevel = ResolveRiskLevel(request, isProd, missingControls);

        return new GovernanceDecision(
            decision,
            riskLevel,
            decision == "Approved",
            missingControls.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            requiredActions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            BuildSummary(decision, riskLevel, missingControls));
    }

    private static string ResolveRiskLevel(AgentDeploymentRequest request, bool isProd, IReadOnlyCollection<string> missingControls)
    {
        if (request.UsesSensitiveData || (isProd && missingControls.Count > 0))
        {
            return "High";
        }

        if (request.UsesInternalData || request.UsesExternalApis)
        {
            return "Medium";
        }

        if (!request.UsesInternalData &&
            !request.UsesSensitiveData &&
            !request.UsesExternalApis &&
            string.Equals(request.TargetEnvironment, "dev", StringComparison.OrdinalIgnoreCase))
        {
            return "Low";
        }

        return "Medium";
    }

    private static string BuildSummary(string decision, string riskLevel, IReadOnlyCollection<string> missingControls)
    {
        if (missingControls.Count == 0)
        {
            return $"Deployment {decision}. Risk level is {riskLevel}. All required controls are present.";
        }

        return $"Deployment {decision}. Risk level is {riskLevel}. Missing controls: {string.Join(", ", missingControls.Distinct(StringComparer.OrdinalIgnoreCase))}.";
    }
}
