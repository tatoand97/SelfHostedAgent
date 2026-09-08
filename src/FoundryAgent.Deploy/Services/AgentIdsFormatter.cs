namespace FoundryAgent.Deploy.Services;

public static class AgentIdsFormatter
{
    public static string Format(IEnumerable<AgentVersion> versions)
        => string.Join(",", versions.Select(agent => $"{agent.Name}:{agent.Version}"));

    public static string SetVariableCommand(string agentIds)
        => $"##vso[task.setvariable variable=AgentIds]{agentIds}";
}