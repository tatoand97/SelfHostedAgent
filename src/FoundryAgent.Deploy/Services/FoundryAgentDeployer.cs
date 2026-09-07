using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using FoundryAgent.Deploy.Agents;
using FoundryAgent.Deploy.Configuration;

namespace FoundryAgent.Deploy.Services;

public sealed class FoundryAgentDeployer
{
    public async Task<IReadOnlyList<ProjectsAgentVersion>> DeployAsync(
        IReadOnlyList<AgentSpecification> agents)
    {
        ArgumentNullException.ThrowIfNull(agents);
        if (agents.Count == 0)
        {
            throw new InvalidOperationException("AgentCatalog must contain at least one agent.");
        }

        // Validate the whole catalog before any remote creation can succeed partially.
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var agent in agents)
        {
            ArgumentNullException.ThrowIfNull(agent);
            agent.Validate();
            if (!names.Add(agent.Name))
            {
                throw new InvalidOperationException($"Duplicate agent name in AgentCatalog: {agent.Name}");
            }
        }

        AIProjectClient projectClient = new(
            new Uri(FoundryConfiguration.ProjectEndpoint),
            new DefaultAzureCredential());
        AgentAdministrationClient agentsClient = projectClient.AgentAdministrationClient;
        List<ProjectsAgentVersion> versions = [];

        foreach (var agent in agents)
        {
            string model = agent.ModelDeploymentName ?? FoundryConfiguration.ModelDeploymentName;
            Console.WriteLine($"Creating Foundry agent: {agent.Name}");
            Console.WriteLine($"Model deployment: {model}");

            DeclarativeAgentDefinition definition = new(model)
            {
                Instructions = agent.Instructions
            };

            ProjectsAgentVersion created = await agentsClient.CreateAgentVersionAsync(
                agentName: agent.Name,
                options: new ProjectsAgentVersionCreationOptions(definition));
            versions.Add(created);

            Console.WriteLine("Agent created successfully");
            Console.WriteLine($"Agent name: {created.Name}");
            Console.WriteLine($"Agent version: {created.Version}");
            Console.WriteLine($"Agent id: {created.Id}");
        }

        return versions;
    }
}
