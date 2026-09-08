using FoundryAgent.Deploy.Agents;

namespace FoundryAgent.Deploy.UnitTests.Agents;

public sealed class AgentCatalogTests
{
    [Fact]
    public void All_WhenRead_ContainsAgents()
    {
        // Arrange / Act
        var agents = AgentCatalog.All;

        // Assert
        Assert.NotNull(agents);
        Assert.NotEmpty(agents);
    }

    [Fact]
    public void All_WhenRead_HasUniqueNonEmptyNames()
    {
        // Arrange
        var agents = AgentCatalog.All;

        // Act
        var names = agents.Select(agent => agent.Name).ToList();

        // Assert
        Assert.All(names, name => Assert.False(string.IsNullOrWhiteSpace(name)));
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void All_WhenRead_HasNonEmptyInstructions()
    {
        // Arrange / Act
        var agents = AgentCatalog.All;

        // Assert
        Assert.All(agents, agent => Assert.False(string.IsNullOrWhiteSpace(agent.Instructions)));
    }

    [Fact]
    public void All_WhenValidated_ContainsValidSpecifications()
    {
        // Arrange
        var agents = AgentCatalog.All;

        // Act
        var exceptions = agents.Select(agent => Record.Exception(() => agent.Validate("global-model"))).ToList();

        // Assert
        Assert.All(exceptions, exception => Assert.Null(exception));
    }
}
