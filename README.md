# Foundry Agent Deploy

## Proposito
Este repositorio crea un agente administrado en Microsoft Foundry desde Azure DevOps Pipeline.

## Flujo
Git -> Azure DevOps Pipeline -> FoundryAgent.Deploy -> Microsoft Foundry Agent Service

## Archivos principales
- agents/support-agent/agent.yaml
- agents/support-agent/instructions.md
- src/FoundryAgent.Deploy
- azure-pipelines.yml

## Variables requeridas
- FOUNDRY_PROJECT_ENDPOINT
- FOUNDRY_MODEL_DEPLOYMENT_NAME

## Ejecucion local
```powershell
az login
dotnet restore
dotnet build
dotnet run --project src/FoundryAgent.Deploy -- agents/support-agent/agent.yaml
```

## Ejecucion desde pipeline
El pipeline ejecuta la console app usando una Azure service connection con workload identity federation.

## Que no contiene
- API self-hosted
- AKS
- APIM
- Docker
- Terraform
- Bicep
- validaciones complejas
