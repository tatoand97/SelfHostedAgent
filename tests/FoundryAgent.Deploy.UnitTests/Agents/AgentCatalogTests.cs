using FoundryAgent.Deploy.Agents;

namespace FoundryAgent.Deploy.UnitTests.Agents;

public sealed class AgentCatalogTests
{
    [Fact]
    public void All_IsNotNullAndContainsAgents()
    {
        Assert.NotNull(AgentCatalog.All);
        Assert.NotEmpty(AgentCatalog.All);
    }

    [Fact]
    public void All_HasUniqueNonEmptyNames()
    {
        var names = AgentCatalog.All.Select(agent => agent.Name).ToList();

        Assert.All(names, name => Assert.False(string.IsNullOrWhiteSpace(name)));
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void All_HasNonEmptyInstructions()
    {
        Assert.All(AgentCatalog.All, agent => Assert.False(string.IsNullOrWhiteSpace(agent.Instructions)));
    }

    [Fact]
    public void All_ContainsValidSpecifications()
    {
        Assert.All(AgentCatalog.All, agent => agent.Validate("gpt-4o"));
    }

    [Fact]
    public void All_ContainsSupportAgent()
    {
        Assert.Contains(AgentCatalog.All, agent =>
            string.Equals(agent.Name, "support-agent", StringComparison.OrdinalIgnoreCase));
    }
}
