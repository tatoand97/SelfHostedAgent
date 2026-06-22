using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using SelfHostedAgent.Api.Models;
using SelfHostedAgent.Api.Options;

namespace SelfHostedAgent.Api.Services;

public sealed class FoundryChatService : IFoundryChatService
{
    private const string AuthenticationMode = "DefaultAzureCredential";

    private readonly IWebHostEnvironment _environment;
    private readonly AzureOpenAIOptions _options;
    private readonly ILogger<FoundryChatService> _logger;

    public FoundryChatService(
        IWebHostEnvironment environment,
        IOptions<AzureOpenAIOptions> options,
        ILogger<FoundryChatService> logger)
    {
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> SendAsync(string question, string businessContext, CancellationToken cancellationToken)
    {
        var endpoint = AzureOpenAIOptions.ResolveEndpoint(_options);
        var deploymentName = AzureOpenAIOptions.ResolveDeploymentName(_options);

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("Missing AZURE_OPENAI_ENDPOINT or AzureOpenAI:Endpoint.");
        }

        if (string.IsNullOrWhiteSpace(deploymentName))
        {
            throw new InvalidOperationException("Missing AZURE_OPENAI_DEPLOYMENT_NAME or AzureOpenAI:DeploymentName.");
        }

        try
        {
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
            });
            var client = new AzureOpenAIClient(new Uri(endpoint), credential);
            var chatClient = client.GetChatClient(deploymentName);

            var completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(await ReadSystemPromptAsync(cancellationToken)),
                    new UserChatMessage($"Contexto de negocio:\n{businessContext}"),
                    new UserChatMessage(question)
                ],
                cancellationToken: cancellationToken);

            return completion.Value.Content.Count > 0
                ? completion.Value.Content[0].Text
                : string.Empty;
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Azure authentication failed.");
            throw new InvalidOperationException("Azure authentication failed. Run az login locally or verify Workload Identity, AZURE_CLIENT_ID and RBAC in AKS.", ex);
        }
        catch (RequestFailedException ex) when (ex.Status is 401 or 403)
        {
            _logger.LogError(ex, "Azure OpenAI authorization failed.");
            throw new InvalidOperationException("Azure OpenAI authorization failed. Verify RBAC: Cognitive Services OpenAI User on the target resource.", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure OpenAI request failed.");
            throw new InvalidOperationException($"Azure OpenAI request failed with status {ex.Status}. Verify endpoint, deployment name and Azure AI Foundry / Azure OpenAI availability.", ex);
        }
        catch (UriFormatException ex)
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not a valid URI.", ex);
        }
    }

    public FoundryStatusResponse GetStatus()
    {
        var endpointConfigured = !string.IsNullOrWhiteSpace(AzureOpenAIOptions.ResolveEndpoint(_options));
        var deploymentConfigured = !string.IsNullOrWhiteSpace(AzureOpenAIOptions.ResolveDeploymentName(_options));
        var configured = endpointConfigured && deploymentConfigured;

        var message = configured
            ? "Foundry configuration is available."
            : BuildMissingConfigurationMessage(endpointConfigured, deploymentConfigured);

        return new FoundryStatusResponse(
            configured,
            endpointConfigured,
            deploymentConfigured,
            AuthenticationMode,
            message);
    }

    private async Task<string> ReadSystemPromptAsync(CancellationToken cancellationToken)
    {
        var promptPath = Path.Combine(_environment.ContentRootPath, "Prompts", "system-prompt.md");

        if (File.Exists(promptPath))
        {
            return await File.ReadAllTextAsync(promptPath, cancellationToken);
        }

        return """
            Eres SelfHostedAgent, un agente self-hosted de soporte interno para Contoso Retail.
            Responde usando unicamente el contexto de negocio proporcionado.
            Si la respuesta no esta en el contexto, indica que no tienes informacion suficiente.
            Responde de forma breve, clara y util.
            No inventes politicas.
            No expongas detalles tecnicos internos.
            No menciones secretos, credenciales ni configuracion de infraestructura.
            """;
    }

    private static string BuildMissingConfigurationMessage(bool endpointConfigured, bool deploymentConfigured)
    {
        if (!endpointConfigured)
        {
            return "Missing AZURE_OPENAI_ENDPOINT.";
        }

        if (!deploymentConfigured)
        {
            return "Missing AZURE_OPENAI_DEPLOYMENT_NAME.";
        }

        return "Foundry configuration is not available.";
    }
}
