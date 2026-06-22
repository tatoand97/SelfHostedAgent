using Azure;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using SelfHostedAgent.Api.Models;
using SelfHostedAgent.Api.Options;

#pragma warning disable OPENAI001

namespace SelfHostedAgent.Api.Services;

public sealed class FoundryChatService : IFoundryChatService
{
    private const string AuthenticationMode = "DefaultAzureCredential";
    private const string Provider = "Azure AI Foundry";

    private readonly IWebHostEnvironment _environment;
    private readonly FoundryOptions _options;
    private readonly ILogger<FoundryChatService> _logger;

    public FoundryChatService(
        IWebHostEnvironment environment,
        IOptions<FoundryOptions> options,
        ILogger<FoundryChatService> logger)
    {
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> SendAsync(string question, string businessContext, CancellationToken cancellationToken)
    {
        var projectEndpoint = FoundryOptions.ResolveProjectEndpoint(_options);
        var modelDeploymentName = FoundryOptions.ResolveModelDeploymentName(_options);

        if (string.IsNullOrWhiteSpace(projectEndpoint))
        {
            throw new InvalidOperationException("Missing FOUNDRY_PROJECT_ENDPOINT or Foundry:ProjectEndpoint.");
        }

        if (string.IsNullOrWhiteSpace(modelDeploymentName))
        {
            throw new InvalidOperationException("Missing FOUNDRY_MODEL_DEPLOYMENT_NAME or Foundry:ModelDeploymentName.");
        }

        try
        {
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
            });

            AIProjectClient projectClient = new(new Uri(projectEndpoint), credential);
            var responsesClient = projectClient.ProjectOpenAIClient.GetProjectResponsesClientForModel(modelDeploymentName);
            var response = await responsesClient.CreateResponseAsync(new CreateResponseOptions
            {
                Instructions = await ReadSystemPromptAsync(cancellationToken),
                InputItems =
                {
                    ResponseItem.CreateUserMessageItem($"""
                        Contexto de negocio:
                        {businessContext}

                        Pregunta del usuario:
                        {question}
                        """)
                }
            }, cancellationToken);

            return response.Value.GetOutputText();
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Azure authentication failed.");
            throw new InvalidOperationException("Azure authentication failed. Run az login locally or verify Workload Identity, AZURE_CLIENT_ID and RBAC in AKS.", ex);
        }
        catch (RequestFailedException ex) when (ex.Status is 401 or 403)
        {
            _logger.LogError(ex, "Azure AI Foundry authorization failed.");
            throw new InvalidOperationException("Azure AI Foundry authorization failed. Verify RBAC for the identity on the Foundry project or related resource.", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure AI Foundry request failed.");
            throw new InvalidOperationException($"Azure AI Foundry request failed with status {ex.Status}. Verify project endpoint, model deployment name and Azure AI Foundry availability.", ex);
        }
        catch (UriFormatException ex)
        {
            throw new InvalidOperationException("Foundry Project Endpoint is not a valid URI.", ex);
        }
    }

    public FoundryStatusResponse GetStatus()
    {
        var projectEndpointConfigured = !string.IsNullOrWhiteSpace(FoundryOptions.ResolveProjectEndpoint(_options));
        var modelDeploymentConfigured = !string.IsNullOrWhiteSpace(FoundryOptions.ResolveModelDeploymentName(_options));
        var configured = projectEndpointConfigured && modelDeploymentConfigured;

        var message = configured
            ? "Foundry configuration is available."
            : BuildMissingConfigurationMessage(projectEndpointConfigured, modelDeploymentConfigured);

        return new FoundryStatusResponse(
            configured,
            projectEndpointConfigured,
            modelDeploymentConfigured,
            Provider,
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
            return "Missing FOUNDRY_PROJECT_ENDPOINT.";
        }

        if (!deploymentConfigured)
        {
            return "Missing FOUNDRY_MODEL_DEPLOYMENT_NAME.";
        }

        return "Foundry configuration is not available.";
    }
}
