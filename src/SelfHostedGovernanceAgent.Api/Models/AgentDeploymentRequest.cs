namespace SelfHostedGovernanceAgent.Api.Models;

public sealed record AgentDeploymentRequest(
    string AgentName,
    string Language,
    string TargetEnvironment,
    bool UsesInternalData,
    bool UsesSensitiveData,
    bool UsesExternalApis,
    bool HasUnitTests,
    bool HasSast,
    bool HasDependencyScan,
    bool HasSecretScan,
    bool HasAiEvaluation,
    bool UsesManagedIdentity);
