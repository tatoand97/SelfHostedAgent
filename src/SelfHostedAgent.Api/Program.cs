using Microsoft.OpenApi;
using SelfHostedAgent.Api.Models;
using SelfHostedAgent.Api.Options;
using SelfHostedAgent.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SelfHostedAgent API",
        Version = "1.0.0-poc",
        Description = "Self-hosted support agent API for Azure AI Foundry Project Endpoint."
    });
});

builder.Services.AddHealthChecks();
builder.Services.Configure<FoundryOptions>(
    builder.Configuration.GetSection(FoundryOptions.SectionName));
builder.Services.AddSingleton<IAgentService, AgentService>();
builder.Services.AddSingleton<IFoundryChatService, FoundryChatService>();
builder.Services.AddSingleton<IBusinessContextService, BusinessContextService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("Health")
    .WithTags("Health");

var agent = app.MapGroup("/api/agent")
    .WithTags("SelfHostedAgent");

agent.MapGet("/metadata", () => Results.Ok(new AgentMetadataResponse(
    "SelfHostedAgent",
    "1.0.0-poc",
    "self-hosted",
    "AKS",
    "Azure APIM",
    "Azure AI Foundry",
    "Foundry Project Endpoint",
    "DefaultAzureCredential / Workload Identity")))
.WithName("GetAgentMetadata");

agent.MapGet("/foundry/status", (IFoundryChatService foundryChatService) =>
    Results.Ok(foundryChatService.GetStatus()))
.WithName("GetFoundryStatus");

agent.MapPost("/invoke", async (
    AgentRequest? request,
    IAgentService agentService,
    CancellationToken cancellationToken) =>
{
    if (request is null)
    {
        return Results.BadRequest(new { error = "request body is required." });
    }

    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.BadRequest(new { error = "question is required." });
    }

    try
    {
        var response = await agentService.InvokeAsync(request, cancellationToken);
        return Results.Ok(response);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            title: "Azure AI Foundry request failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("InvokeAgent");

app.Run();
