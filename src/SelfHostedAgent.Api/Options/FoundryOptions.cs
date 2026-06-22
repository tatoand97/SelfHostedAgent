namespace SelfHostedAgent.Api.Options;

public sealed class FoundryOptions
{
    public const string SectionName = "Foundry";

    public string ProjectEndpoint { get; set; } = string.Empty;

    public string ModelDeploymentName { get; set; } = string.Empty;

    public static string ResolveProjectEndpoint(FoundryOptions options)
    {
        return ResolveSetting("FOUNDRY_PROJECT_ENDPOINT", options.ProjectEndpoint);
    }

    public static string ResolveModelDeploymentName(FoundryOptions options)
    {
        return ResolveSetting("FOUNDRY_MODEL_DEPLOYMENT_NAME", options.ModelDeploymentName);
    }

    private static string ResolveSetting(string environmentVariableName, string configuredValue)
    {
        var environmentValue = Environment.GetEnvironmentVariable(environmentVariableName);

        return string.IsNullOrWhiteSpace(environmentValue)
            ? configuredValue
            : environmentValue;
    }
}
