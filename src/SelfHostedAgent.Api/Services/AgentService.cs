using SelfHostedAgent.Api.Models;
using SelfHostedAgent.Api.Options;

namespace SelfHostedAgent.Api.Services;

public sealed class AgentService : IAgentService
{
    private const string AgentName = "SelfHostedAgent";
    private const string AuthenticationMode = "DefaultAzureCredential";

    private readonly IBusinessContextService _businessContextService;
    private readonly IFoundryChatService _foundryChatService;
    private readonly FoundryOptions _options;

    public AgentService(
        IBusinessContextService businessContextService,
        IFoundryChatService foundryChatService,
        Microsoft.Extensions.Options.IOptions<FoundryOptions> options)
    {
        _businessContextService = businessContextService;
        _foundryChatService = foundryChatService;
        _options = options.Value;
    }

    public async Task<AgentResponse> InvokeAsync(AgentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ArgumentException("question is required.", nameof(request));
        }

        var businessContext = _businessContextService.GetBusinessContext();
        var answer = await _foundryChatService.SendAsync(request.Question, businessContext, cancellationToken);

        return new AgentResponse(
            answer,
            AgentName,
            FoundryOptions.ResolveModelDeploymentName(_options),
            request.CorrelationId,
            UsedBusinessContext: true,
            AuthenticationMode);
    }
}
