using SelfHostedGovernanceAgent.Api.Models;
using SelfHostedGovernanceAgent.Api.Services;

namespace SelfHostedGovernanceAgent.Tests;

public sealed class GovernanceEvaluationServiceTests
{
    private readonly GovernanceEvaluationService _service = new();

    [Fact]
    public void Evaluate_Blocks_WhenAgentNameIsEmpty()
    {
        var decision = _service.Evaluate(ValidRequest() with { AgentName = "" });

        Assert.Equal("Blocked", decision.Decision);
        Assert.False(decision.CanDeploy);
        Assert.Contains("agentName", decision.MissingControls);
    }

    [Fact]
    public void Evaluate_Blocks_ProdWithoutSast()
    {
        var decision = _service.Evaluate(ValidProdRequest() with { HasSast = false });

        Assert.Equal("Blocked", decision.Decision);
        Assert.False(decision.CanDeploy);
        Assert.Contains("SAST", decision.MissingControls);
    }

    [Fact]
    public void Evaluate_Blocks_ProdWithoutUnitTests()
    {
        var decision = _service.Evaluate(ValidProdRequest() with { HasUnitTests = false });

        Assert.Equal("Blocked", decision.Decision);
        Assert.False(decision.CanDeploy);
        Assert.Contains("UnitTests", decision.MissingControls);
    }

    [Fact]
    public void Evaluate_Blocks_SensitiveDataWithoutManagedIdentity()
    {
        var decision = _service.Evaluate(ValidRequest() with
        {
            UsesSensitiveData = true,
            UsesManagedIdentity = false
        });

        Assert.Equal("Blocked", decision.Decision);
        Assert.False(decision.CanDeploy);
        Assert.Contains("ManagedIdentity", decision.MissingControls);
    }

    [Fact]
    public void Evaluate_NeedsReview_ExternalApisWithoutDependencyScan()
    {
        var decision = _service.Evaluate(ValidRequest() with
        {
            UsesExternalApis = true,
            HasDependencyScan = false
        });

        Assert.Equal("NeedsReview", decision.Decision);
        Assert.False(decision.CanDeploy);
        Assert.Contains("DependencyScan", decision.MissingControls);
    }

    [Fact]
    public void Evaluate_Approves_ValidDevRequest()
    {
        var decision = _service.Evaluate(ValidRequest());

        Assert.Equal("Approved", decision.Decision);
        Assert.True(decision.CanDeploy);
        Assert.Equal("Low", decision.RiskLevel);
        Assert.Empty(decision.MissingControls);
    }

    [Fact]
    public void Evaluate_Approves_ValidProdRequestWithAllControls()
    {
        var decision = _service.Evaluate(ValidProdRequest());

        Assert.Equal("Approved", decision.Decision);
        Assert.True(decision.CanDeploy);
        Assert.Empty(decision.MissingControls);
    }

    private static AgentDeploymentRequest ValidRequest()
    {
        return new AgentDeploymentRequest(
            AgentName: "GovernanceAgent",
            Language: "dotnet",
            TargetEnvironment: "dev",
            UsesInternalData: false,
            UsesSensitiveData: false,
            UsesExternalApis: false,
            HasUnitTests: true,
            HasSast: true,
            HasDependencyScan: true,
            HasSecretScan: true,
            HasAiEvaluation: true,
            UsesManagedIdentity: true);
    }

    private static AgentDeploymentRequest ValidProdRequest()
    {
        return ValidRequest() with
        {
            TargetEnvironment = "prod",
            UsesInternalData = true,
            UsesExternalApis = true
        };
    }
}
