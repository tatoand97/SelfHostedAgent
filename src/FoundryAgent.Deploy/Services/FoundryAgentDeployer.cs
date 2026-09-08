using FoundryAgent.Deploy.Agents;
using FoundryAgent.Deploy.Configuration;

namespace FoundryAgent.Deploy.Services;

public sealed class FoundryAgentDeployer
{
    private readonly IAgentVersionCreator? agentVersionCreator;
    private readonly string globalModelDeploymentName;

    public FoundryAgentDeployer(
        IAgentVersionCreator? agentVersionCreator = null,
        string? globalModelDeploymentName = null)
    {
        this.agentVersionCreator = agentVersionCreator;
        this.globalModelDeploymentName = globalModelDeploymentName ?? FoundryConfiguration.ModelDeploymentName;
    }

    public async Task<IReadOnlyList<AgentVersion>> DeployAsync(
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
            agent.Validate(globalModelDeploymentName);
            if (!names.Add(agent.Name))
            {
                throw new InvalidOperationException($"Duplicate agent name in AgentCatalog: {agent.Name}");
            }
        }

        IAgentVersionCreator creator = agentVersionCreator ?? new AzureAgentVersionCreator();
        List<AgentVersion> versions = [];

        foreach (var agent in agents)
        {
            string model = agent.ModelDeploymentName ?? globalModelDeploymentName;
            Console.WriteLine($"Creating Foundry agent: {agent.Name}");
            Console.WriteLine($"Model deployment: {model}");

            AgentVersion created = await creator.CreateAsync(
                new AgentVersionCreationRequest(agent.Name, agent.Instructions, model));
            versions.Add(created);

            Console.WriteLine("Agent created successfully");
            Console.WriteLine($"Agent name: {created.Name}");
            Console.WriteLine($"Agent version: {created.Version}");
            Console.WriteLine($"Agent id: {created.Id}");
        }

        return versions;
    }
}
