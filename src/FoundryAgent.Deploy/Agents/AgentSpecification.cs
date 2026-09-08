using FoundryAgent.Deploy.Configuration;

namespace FoundryAgent.Deploy.Agents;

public sealed record AgentSpecification(
    string Name,
    string Instructions,
    string? ModelDeploymentName = null)
{
    public void Validate(string? globalModelDeploymentName = null)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Every agent must have a non-empty Name in AgentCatalog.cs.");
        }

        // Keep evaluation identifiers and logging commands on a single unambiguous line.
        if (Name.Any(character => char.IsWhiteSpace(character) || char.IsControl(character)
            || character is ':' or ',' or '%'))
        {
            throw new InvalidOperationException("Agent names cannot contain whitespace, control characters, ':', ',' or '%'.");
        }

        if (string.IsNullOrWhiteSpace(Instructions))
        {
            throw new InvalidOperationException($"Agent '{Name}' must have non-empty Instructions in AgentCatalog.cs.");
        }

        FoundryConfiguration.ValidateModelDeployment(ModelDeploymentName ?? globalModelDeploymentName ?? FoundryConfiguration.ModelDeploymentName);
    }
}
