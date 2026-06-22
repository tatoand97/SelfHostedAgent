using SelfHostedAgent.Api.Models;

namespace SelfHostedAgent.Api.Services;

public interface IFoundryChatService
{
    Task<string> SendAsync(string question, string businessContext, CancellationToken cancellationToken);

    FoundryStatusResponse GetStatus();
}
