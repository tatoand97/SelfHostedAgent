using SelfHostedGovernanceAgent.Api.Models;

namespace SelfHostedGovernanceAgent.Api.Services;

public interface IFoundryChatService
{
    Task<GovernanceChatResponse> AskAsync(GovernanceChatRequest request, CancellationToken cancellationToken);
}
