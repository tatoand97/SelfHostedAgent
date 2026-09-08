namespace FoundryAgent.Deploy.Services;

public sealed record AgentVersionCreationRequest(
    string Name,
    string Instructions,
    string ModelDeploymentName);

public sealed record AgentVersion(string Name, string Version, string Id);

public interface IAgentVersionCreator
{
    Task<AgentVersion> CreateAsync(
        AgentVersionCreationRequest request,
        CancellationToken cancellationToken = default);
}