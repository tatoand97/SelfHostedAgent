using FoundryAgent.Deploy.Agents;
using FoundryAgent.Deploy.Configuration;
using FoundryAgent.Deploy.Services;

try
{
    FoundryConfiguration.Validate();
    var agents = AgentCatalog.All;
    var versions = await new FoundryAgentDeployer().DeployAsync(agents);
    string agentIds = AgentIdsFormatter.Format(versions);

    Console.WriteLine($"Agents created for evaluation: {agentIds}");
    Console.WriteLine(AgentIdsFormatter.SetVariableCommand(agentIds));
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine("Foundry agent deployment failed. AgentIds was not updated.");
    Console.Error.WriteLine(exception);
    return 1;
}
