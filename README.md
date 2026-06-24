# Foundry Agent Deploy

## Proposito

Este repositorio contiene una console app .NET minima para crear una version de un agente administrado en Microsoft Foundry a partir de un manifiesto YAML y un archivo de instrucciones Markdown.

## Flujo

Manifest YAML + instructions.md -> FoundryAgent.Deploy -> Microsoft Foundry Agent Service

## Archivos principales

- agents/support-agent/agent.yaml
- agents/support-agent/instructions.md
- src/FoundryAgent.Deploy
- SelfHostedAgent.slnx

## Variables requeridas

- FOUNDRY_PROJECT_ENDPOINT
- FOUNDRY_MODEL_DEPLOYMENT_NAME

## Ejecucion local

```powershell
az login
dotnet restore SelfHostedAgent.slnx
dotnet build SelfHostedAgent.slnx
dotnet run --project src/FoundryAgent.Deploy -- agents/support-agent/agent.yaml
```

## Que no contiene

- API self-hosted
- endpoints HTTP
- AKS
- APIM
- Docker
- Terraform
- Bicep
- definicion de pipeline
- validaciones complejas
