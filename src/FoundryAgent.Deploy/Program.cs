using Azure;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

if (args.Length == 0)
{
    Fail("Manifest path argument is required.");
}

string manifestPath = Path.GetFullPath(args[0]);
if (!File.Exists(manifestPath))
{
    Fail($"Manifest file not found: {manifestPath}");
}

string projectEndpoint = RequiredEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT");
string modelDeploymentName = RequiredEnvironmentVariable("FOUNDRY_MODEL_DEPLOYMENT_NAME");

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();

AgentManifest manifest = deserializer.Deserialize<AgentManifest>(File.ReadAllText(manifestPath))
    ?? throw new InvalidOperationException("Manifest could not be read.");

string manifestDirectory = Path.GetDirectoryName(manifestPath) ?? Directory.GetCurrentDirectory();
string instructionsPath = Path.GetFullPath(Path.Combine(manifestDirectory, manifest.InstructionsFile));
if (!File.Exists(instructionsPath))
{
    Fail($"Instructions file not found: {instructionsPath}");
}

string instructions = File.ReadAllText(instructionsPath);
string model = ResolveEnvironmentPlaceholder(manifest.Model, modelDeploymentName);

var credential = new DefaultAzureCredential();
AIProjectClient projectClient = new(new Uri(projectEndpoint), credential);

// The current Azure.AI.Projects.Agents SDK exposes declarative prompt agents through AgentAdministrationClient.
AgentAdministrationClient agentsClient = new(new Uri(projectEndpoint), credential);
DeclarativeAgentDefinition definition = new(model)
{
    Instructions = instructions
};

Console.WriteLine($"agent name: {manifest.Name}");
Console.WriteLine($"display name: {manifest.DisplayName}");
Console.WriteLine($"model deployment: {model}");
Console.WriteLine($"Foundry project endpoint: {projectEndpoint}");

bool agentExists = await AgentExistsAsync(agentsClient, manifest.Name);
ProjectsAgentVersion deployed = await agentsClient.CreateAgentVersionAsync(
    agentName: manifest.Name,
    options: new ProjectsAgentVersionCreationOptions(definition));

Console.WriteLine($"result: {(agentExists ? "updated" : "created")}");
Console.WriteLine($"agent id: {deployed.Id}");
Console.WriteLine($"agent version: {deployed.Version}");

static string RequiredEnvironmentVariable(string name)
{
    string? value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value))
    {
        Fail($"Environment variable {name} is required.");
    }

    return value ?? "";
}

static async Task<bool> AgentExistsAsync(AgentAdministrationClient agentsClient, string agentName)
{
    try
    {
        _ = await agentsClient.GetAgentAsync(agentName);
        return true;
    }
    catch (RequestFailedException ex) when (ex.Status == 404)
    {
        return false;
    }
}

static string ResolveEnvironmentPlaceholder(string value, string modelDeploymentName)
{
    return value.Replace("${FOUNDRY_MODEL_DEPLOYMENT_NAME}", modelDeploymentName, StringComparison.Ordinal);
}

static void Fail(string message)
{
    Console.Error.WriteLine(message);
    Environment.Exit(1);
}

internal sealed class AgentManifest
{
    public string Name { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Description { get; init; } = "";
    public string Model { get; init; } = "";
    public string InstructionsFile { get; init; } = "";
}
