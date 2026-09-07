# Foundry Agent Deploy

Plantilla mínima de consola en .NET 10 para crear y versionar Prompt Agents administrados por Microsoft Foundry, 100 % code-first desde C# mediante Azure.AI.Projects. Cada ejecución crea una nueva versión de cada agente del catálogo.

## Requisitos

- SDK .NET 10 para compilar y runtime .NET 10 para ejecutar el DLL.
- Identidad autenticada disponible durante la ejecución, con permisos para crear agentes en el proyecto de Foundry. El programa utiliza `DefaultAzureCredential` con la identidad disponible, sin solicitar credenciales ni realizar login manual.

## Configuración y agentes

En `src/FoundryAgent.Deploy/Configuration/FoundryConfiguration.cs`, sustituye los placeholders de `ProjectEndpoint` (URL HTTPS del proyecto) y `ModelDeploymentName` (nombre del deployment del modelo). Son valores no secretos escritos en C#; recompila después de cambiarlos.

Agrega o modifica entradas `AgentSpecification` en `src/FoundryAgent.Deploy/Agents/AgentCatalog.cs`. El ejemplo `support-agent` incluye sus instrucciones directamente en un raw string literal. Para usar otro modelo en un agente, proporciona su `ModelDeploymentName` opcional; si es `null`, se usa el de `FoundryConfiguration`.

## Compilar y ejecutar

Desde la raíz del repositorio:

```powershell
dotnet restore
dotnet build -c Release
dotnet src/FoundryAgent.Deploy/bin/Release/net10.0/FoundryAgent.Deploy.dll
```

## Evaluación en Azure DevOps

Solo después de crear correctamente todos los agentes, el ejecutable escribe un único logging command con sus nombres y versiones recién creadas:

```text
Agents created for evaluation: support-agent:3
##vso[task.setvariable variable=AgentIds]support-agent:3
```

Para varios agentes, el valor tiene el formato `support-agent:3,collections-agent:5`. Una tarea posterior `AIAgentEvaluation@2` del mismo job puede consumir `AgentIds`. El ejecutable no lee el valor anterior ni variables de configuración del pipeline.

Si falla la validación o cualquier creación, termina con código 1, escribe la excepción con su stack trace en stderr y no establece `AgentIds`. Las versiones que ya se hubieran creado antes de un fallo parcial permanecen en Foundry. En caso de éxito termina con código 0.
