namespace SelfHostedAgent.Api.Options;

public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    public string Endpoint { get; set; } = string.Empty;

    public string DeploymentName { get; set; } = string.Empty;

    public static string ResolveEndpoint(AzureOpenAIOptions options)
    {
        return ResolveSetting("AZURE_OPENAI_ENDPOINT", options.Endpoint);
    }

    public static string ResolveDeploymentName(AzureOpenAIOptions options)
    {
        return ResolveSetting("AZURE_OPENAI_DEPLOYMENT_NAME", options.DeploymentName);
    }

    private static string ResolveSetting(string environmentVariableName, string configuredValue)
    {
        var environmentValue = Environment.GetEnvironmentVariable(environmentVariableName);
        return string.IsNullOrWhiteSpace(environmentValue) ? configuredValue : environmentValue;
    }
}
