using Microsoft.OpenApi;
using SelfHostedGovernanceAgent.Api.Models;
using SelfHostedGovernanceAgent.Api.Options;
using SelfHostedGovernanceAgent.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SelfHostedGovernanceAgent API",
        Version = "1.0.0-poc",
        Description = "Self-hosted GovernanceAgent API for Azure AI Foundry / Azure OpenAI."
    });
});

builder.Services.AddHealthChecks();
builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));
builder.Services.AddSingleton<IGovernanceEvaluationService, GovernanceEvaluationService>();
builder.Services.AddSingleton<IFoundryChatService, FoundryChatService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok())
    .WithName("Health")
    .WithTags("Health");

var governance = app.MapGroup("/api/agents/governance")
    .WithTags("GovernanceAgent");

governance.MapGet("/version", () => Results.Ok(new
{
    name = "GovernanceAgent",
    version = "1.0.0-poc",
    hostingType = "self-hosted",
    runtimeTarget = "AKS",
    exposedBy = "Azure APIM",
    foundryEnabled = true,
    authentication = "DefaultAzureCredential / Workload Identity"
}))
.WithName("GetGovernanceAgentVersion");

governance.MapPost("/chat", async (
    GovernanceChatRequest request,
    IFoundryChatService chatService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "message is required." });
    }

    try
    {
        var response = await chatService.AskAsync(request, cancellationToken);
        return Results.Ok(response);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            title: "Azure OpenAI request failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("ChatWithGovernanceAgent");

governance.MapPost("/evaluate", (
    AgentDeploymentRequest request,
    IGovernanceEvaluationService evaluationService) =>
{
    if (request is null)
    {
        return Results.BadRequest(new { error = "request body is required." });
    }

    var decision = evaluationService.Evaluate(request);
    return Results.Ok(decision);
})
.WithName("EvaluateAgentDeployment");

app.Run();
