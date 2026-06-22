# SelfHostedAgent

PoC funcional de un self-hosted agent en ASP.NET Core Minimal API sobre .NET 10. El agente atiende preguntas simples de soporte interno para Contoso Retail y consume Azure AI Foundry / Azure OpenAI con `DefaultAzureCredential`, sin API keys.

## Alcance

Este repositorio contiene solo el codigo del self-hosted agent, sus pruebas y documentacion basica.

DevOps se encarga de infraestructura, Dockerfile o imagen, ACR, AKS, Azure APIM, Workload Identity, pipeline, despliegue y SAST. No se incluyen Dockerfile, manifiestos AKS, APIM policies, Bicep, Terraform, GitHub Actions ni Azure Pipelines.

## Arquitectura objetivo

```text
Cliente -> Azure APIM -> AKS -> SelfHostedAgent.Api -> Azure AI Foundry / Azure OpenAI
```

## Comandos locales

```powershell
dotnet restore
dotnet build
dotnet test
az login
dotnet run --project src/SelfHostedAgent.Api
```

Swagger queda disponible en `/swagger` cuando la API esta en ejecucion.

## Variables de entorno

```text
AZURE_OPENAI_ENDPOINT
AZURE_OPENAI_DEPLOYMENT_NAME
AZURE_CLIENT_ID opcional para Workload Identity en AKS
```

`appsettings.json` contiene la seccion `AzureOpenAI`, pero no guarda secretos. Las variables de entorno tienen prioridad sobre la configuracion del archivo.

## Ejemplos curl

Health:

```bash
curl http://localhost:5000/health
```

Metadata:

```bash
curl http://localhost:5000/api/agent/metadata
```

Foundry status:

```bash
curl http://localhost:5000/api/agent/foundry/status
```

Invoke:

```bash
curl -X POST http://localhost:5000/api/agent/invoke \
  -H "Content-Type: application/json" \
  -d "{\"question\":\"Cual es la politica de devoluciones?\",\"correlationId\":\"demo-001\"}"
```

## Endpoints

- `GET /health`
- `GET /api/agent/metadata`
- `GET /api/agent/foundry/status`
- `POST /api/agent/invoke`

## Autenticacion

El codigo usa `DefaultAzureCredential` desde `Azure.Identity`.

Localmente funciona con `az login`. En AKS funciona con Workload Identity cuando DevOps configure la identidad y, si aplica, `AZURE_CLIENT_ID`.

No se usan API keys, client secrets ni secretos en archivos de configuracion.

## Nota para DevOps

- El proyecto no incluye Dockerfile.
- El proyecto no incluye pipeline.
- El proyecto no incluye manifiestos AKS.
- El proyecto no incluye APIM policies.
- El pipeline debe agregar esos artefactos.
- El workload debe configurar Workload Identity.
- La identidad debe tener permisos para consumir Azure OpenAI / Foundry, por ejemplo `Cognitive Services OpenAI User` sobre el recurso correspondiente.
