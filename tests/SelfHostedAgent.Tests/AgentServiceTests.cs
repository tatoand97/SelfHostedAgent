using Microsoft.Extensions.Options;
using SelfHostedAgent.Api.Models;
using SelfHostedAgent.Api.Options;
using SelfHostedAgent.Api.Services;

namespace SelfHostedAgent.Tests;

public sealed class AgentServiceTests
{
    [Fact]
    public async Task InvokeAsync_RejectsEmptyQuestion()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.InvokeAsync(new AgentRequest("", null), CancellationToken.None));
    }

    [Fact]
    public async Task InvokeAsync_PreservesCorrelationId()
    {
        var service = CreateService();

        var response = await service.InvokeAsync(new AgentRequest("Cual es el horario?", "corr-123"), CancellationToken.None);

        Assert.Equal("corr-123", response.CorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsSelfHostedAgentName()
    {
        var service = CreateService();

        var response = await service.InvokeAsync(new AgentRequest("Cual es el horario?", null), CancellationToken.None);

        Assert.Equal("SelfHostedAgent", response.AgentName);
    }

    [Fact]
    public async Task InvokeAsync_UsesBusinessContext()
    {
        var businessContext = new FakeBusinessContextService("contexto Contoso Retail");
        var chat = new FakeFoundryChatService();
        var service = CreateService(businessContext, chat);

        var response = await service.InvokeAsync(new AgentRequest("Cual es la politica de devoluciones?", null), CancellationToken.None);

        Assert.True(response.UsedBusinessContext);
        Assert.Equal("contexto Contoso Retail", chat.LastBusinessContext);
    }

    [Fact]
    public async Task InvokeAsync_CallsFoundryChatService_WhenQuestionIsValid()
    {
        var chat = new FakeFoundryChatService();
        var service = CreateService(foundryChatService: chat);

        await service.InvokeAsync(new AgentRequest("Hay disponibilidad?", null), CancellationToken.None);

        Assert.Equal(1, chat.CallCount);
        Assert.Equal("Hay disponibilidad?", chat.LastQuestion);
    }

    private static AgentService CreateService(
        IBusinessContextService? businessContextService = null,
        FakeFoundryChatService? foundryChatService = null)
    {
        var options = Options.Create(new AzureOpenAIOptions { DeploymentName = "test-deployment" });
        return new AgentService(
            businessContextService ?? new FakeBusinessContextService("business-context"),
            foundryChatService ?? new FakeFoundryChatService(),
            options);
    }

    private sealed class FakeBusinessContextService : IBusinessContextService
    {
        private readonly string _businessContext;

        public FakeBusinessContextService(string businessContext)
        {
            _businessContext = businessContext;
        }

        public string GetBusinessContext() => _businessContext;
    }

    private sealed class FakeFoundryChatService : IFoundryChatService
    {
        public int CallCount { get; private set; }

        public string? LastQuestion { get; private set; }

        public string? LastBusinessContext { get; private set; }

        public Task<string> SendAsync(string question, string businessContext, CancellationToken cancellationToken)
        {
            CallCount++;
            LastQuestion = question;
            LastBusinessContext = businessContext;

            return Task.FromResult("respuesta generada");
        }

        public FoundryStatusResponse GetStatus()
        {
            return new FoundryStatusResponse(true, true, true, "DefaultAzureCredential", "Foundry configuration is available.");
        }
    }
}
