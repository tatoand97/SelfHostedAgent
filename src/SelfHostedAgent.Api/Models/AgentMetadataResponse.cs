namespace SelfHostedAgent.Api.Models;

public sealed record AgentMetadataResponse(
    string Name,
    string Version,
    string HostingType,
    string RuntimeTarget,
    string ExposedBy,
    string ModelProvider,
    string ConnectionMode,
    string Authentication);
