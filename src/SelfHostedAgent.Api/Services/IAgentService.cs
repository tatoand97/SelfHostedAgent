using SelfHostedAgent.Api.Models;

namespace SelfHostedAgent.Api.Services;

public interface IAgentService
{
    Task<AgentResponse> InvokeAsync(AgentRequest request, CancellationToken cancellationToken);
}
