# SelfHostedGovernanceAgent

PoC funcional de un agente self-hosted en ASP.NET Core Minimal API sobre .NET 10. La API expone `GovernanceAgent` para responder preguntas de gobierno de despliegue usando Azure AI Foundry / Azure OpenAI y para evaluar solicitudes de despliegue con reglas determinísticas.

## Alcance

Este repositorio implementa solo el código del self-hosted agent.

DevOps se encarga de AKS, Azure APIM, pipeline, SAST, ACR, ingress, Workload Identity y despliegue.

No se incluyen Bicep, Terraform, manifiestos completos de AKS, policies de APIM ni pipeline.

## Arquitectura

```text
Cliente -> Azure APIM -> AKS -> SelfHostedGovernanceAgent.Api -> Azure AI Foundry / Azure OpenAI
```

## Ejecutar localmente

```powershell
dotnet restore
dotnet build
dotnet test
az login
$env:AZURE_OPENAI_ENDPOINT = "https://<resource-name>.openai.azure.com/"
$env:AZURE_OPENAI_DEPLOYMENT_NAME = "<deployment-name>"
dotnet run --project src/SelfHostedGovernanceAgent.Api
```

Swagger queda disponible en `/swagger` cuando la API está en ejecución.

## Variables de entorno

```text
AZURE_OPENAI_ENDPOINT
AZURE_OPENAI_DEPLOYMENT_NAME
AZURE_CLIENT_ID opcional para Workload Identity en AKS
```

`appsettings.json` contiene la sección `AzureOpenAI`, pero no guarda secretos. Las variables de entorno tienen prioridad sobre la configuración del archivo.

## Chat

```bash
curl -X POST http://localhost:5000/api/agents/governance/chat \
  -H "Content-Type: application/json" \
  -d "{\"message\":\"Que controles necesito para desplegar un agente en prod sobre AKS?\"}"
```

## Evaluate

```bash
curl -X POST http://localhost:5000/api/agents/governance/evaluate \
  -H "Content-Type: application/json" \
  -d "{
    \"agentName\":\"GovernanceAgent\",
    \"language\":\"dotnet\",
    \"targetEnvironment\":\"prod\",
    \"usesInternalData\":true,
    \"usesSensitiveData\":false,
    \"usesExternalApis\":true,
    \"hasUnitTests\":true,
    \"hasSast\":true,
    \"hasDependencyScan\":true,
    \"hasSecretScan\":true,
    \"hasAiEvaluation\":true,
    \"usesManagedIdentity\":true
  }"
```

## Endpoints

- `GET /health`
- `GET /api/agents/governance/version`
- `POST /api/agents/governance/chat`
- `POST /api/agents/governance/evaluate`

## Autenticacion

El código usa `DefaultAzureCredential` desde `Azure.Identity`.

Localmente funciona con `az login`. En AKS funciona con Workload Identity cuando DevOps configure la identidad y, si aplica, `AZURE_CLIENT_ID`.

No se usan API keys y no se leen secretos desde configuración.

## Nota para DevOps

- Importar Swagger/OpenAPI en Azure APIM.
- Configurar Workload Identity en AKS.
- Asignar RBAC a la identidad administrada.
- Configurar `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_DEPLOYMENT_NAME` y opcionalmente `AZURE_CLIENT_ID` en el workload.
- Publicar la imagen en ACR.

Rol RBAC conceptual: `Cognitive Services OpenAI User` sobre el recurso Azure OpenAI / Foundry correspondiente.
