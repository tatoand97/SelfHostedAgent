namespace FoundryAgent.Deploy.Configuration;

public static class FoundryConfiguration
{
    public const string ProjectEndpoint = "REPLACE_WITH_PROJECT_ENDPOINT";
    public const string ModelDeploymentName = "REPLACE_WITH_MODEL_DEPLOYMENT_NAME";

    public static void Validate()
    {
        if (string.IsNullOrWhiteSpace(ProjectEndpoint)
            || ProjectEndpoint.StartsWith("REPLACE_WITH_", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Set ProjectEndpoint in FoundryConfiguration.cs before deployment.");
        }

        if (!Uri.TryCreate(ProjectEndpoint, UriKind.Absolute, out var endpoint)
            || endpoint.Scheme != Uri.UriSchemeHttps
            || string.IsNullOrEmpty(endpoint.Host)
            || !string.IsNullOrEmpty(endpoint.UserInfo))
        {
            throw new InvalidOperationException("ProjectEndpoint must be an absolute HTTPS project URL without credentials.");
        }

        ValidateModelDeployment(ModelDeploymentName);
    }

    internal static void ValidateModelDeployment(string model)
    {
        if (string.IsNullOrWhiteSpace(model)
            || model.StartsWith("REPLACE_WITH_", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Set a non-empty model deployment in FoundryConfiguration.cs or AgentCatalog.cs; replace the placeholder.");
        }
    }
}
