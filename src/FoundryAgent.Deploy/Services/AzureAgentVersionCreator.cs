using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using FoundryAgent.Deploy.Configuration;

namespace FoundryAgent.Deploy.Services;

internal sealed class AzureAgentVersionCreator : IAgentVersionCreator
{
    private readonly AgentAdministrationClient agentsClient;

    public AzureAgentVersionCreator()
    {
        AIProjectClient projectClient = new(
            new Uri(FoundryConfiguration.ProjectEndpoint),
            new DefaultAzureCredential());
        agentsClient = projectClient.AgentAdministrationClient;
    }

    public async Task<AgentVersion> CreateAsync(
        AgentVersionCreationRequest request,
        CancellationToken cancellationToken = default)
    {
        DeclarativeAgentDefinition definition = new(request.ModelDeploymentName)
        {
            Instructions = request.Instructions
        };

        ProjectsAgentVersion created = await agentsClient.CreateAgentVersionAsync(
            agentName: request.Name,
            options: new ProjectsAgentVersionCreationOptions(definition),
            cancellationToken: cancellationToken);
        return new AgentVersion(created.Name, created.Version, created.Id);
    }
}