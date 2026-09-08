namespace FoundryAgent.Deploy.Configuration;

public static class FoundryConfiguration
{
    public static string? ProjectEndpoint => Environment.GetEnvironmentVariable("AzureAIProjectEndpoint");
    public static string? ModelDeploymentName => Environment.GetEnvironmentVariable("DeploymentName");

    public static void Validate()
        => Validate(ProjectEndpoint, ModelDeploymentName);

    public static void Validate(string? projectEndpoint, string? modelDeploymentName)
    {
        if (string.IsNullOrWhiteSpace(projectEndpoint)
            || projectEndpoint.StartsWith("REPLACE_WITH_", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Set the AzureAIProjectEndpoint environment variable before deployment.");
        }

        if (!Uri.TryCreate(projectEndpoint, UriKind.Absolute, out var endpoint)
            || endpoint.Scheme != Uri.UriSchemeHttps
            || string.IsNullOrEmpty(endpoint.Host)
            || !string.IsNullOrEmpty(endpoint.UserInfo))
        {
            throw new InvalidOperationException("AzureAIProjectEndpoint must be an absolute HTTPS project URL without credentials.");
        }

        ValidateModelDeployment(modelDeploymentName);
    }

    internal static void ValidateModelDeployment(string? model)
    {
        if (string.IsNullOrWhiteSpace(model)
            || model.StartsWith("REPLACE_WITH_", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Set a non-empty model deployment using DeploymentName or an override in AgentCatalog.cs; replace the placeholder.");
        }
    }
}
