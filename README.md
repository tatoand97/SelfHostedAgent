# Foundry Agent Deploy

Plantilla mínima de consola en .NET 10 para crear y versionar Prompt Agents administrados por Microsoft Foundry, 100 % code-first desde C# mediante Azure.AI.Projects. Cada ejecución crea una nueva versión de cada agente del catálogo.

## Requisitos

- SDK .NET 10 para compilar y runtime .NET 10 para ejecutar el DLL.
- Identidad autenticada disponible durante la ejecución, con permisos para crear agentes en el proyecto de Foundry. El programa utiliza `DefaultAzureCredential` con la identidad disponible, sin solicitar credenciales ni realizar login manual.

## Configuración y agentes

Configura las variables de entorno `AzureAIProjectEndpoint` (URL HTTPS del proyecto) y `DeploymentName` (nombre del deployment del modelo) antes de ejecutar. `src/FoundryAgent.Deploy/Configuration/FoundryConfiguration.cs` obtiene y valida estos valores no secretos. En Visual Studio deben estar disponibles en el entorno del proceso iniciado; en Azure DevOps, en el proceso que ejecuta el DLL.

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

Para varios agentes, el valor tiene el formato `support-agent:3,collections-agent:5`. Una tarea posterior `AIAgentEvaluation@2` del mismo job puede consumir `AgentIds`. El ejecutable no lee el valor anterior de `AgentIds` ni `DataPath`.

Si falla la validación o cualquier creación, termina con código 1, escribe la excepción con su stack trace en stderr y no establece `AgentIds`. Las versiones que ya se hubieran creado antes de un fallo parcial permanecen en Foundry. En caso de éxito termina con código 0.

## Pruebas unitarias y cobertura

El proyecto existente `tests/FoundryAgent.Deploy.UnitTests` usa xUnit v3 mediante `xunit.v3.mtp-off` 4.0.0, el runner Visual Studio 4.0.0, Microsoft.NET.Test.Sdk 18.9.0 y Moq 4.20.72. No utiliza Microsoft.Testing.Platform. Los mocks se limitan a `IAgentVersionCreator`: ninguna prueba ejecuta credenciales, HTTP ni llamadas reales a Azure o Foundry.

Las pruebas de configuración guardan y restauran los valores originales de las variables de entorno en cada caso. Su colección tiene el paralelismo desactivado, incluso respecto de las demás colecciones.

```powershell
dotnet test -c Release
dotnet test tests/FoundryAgent.Deploy.UnitTests/FoundryAgent.Deploy.UnitTests.csproj -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./TestResults/Coverage/
```

La cobertura se genera exclusivamente mediante `coverlet.msbuild` 10.0.1, sin collector, en `tests/FoundryAgent.Deploy.UnitTests/TestResults/Coverage/coverage.cobertura.xml`. Se mide la lógica propia; se excluyen explícitamente `Program.cs` (composition root) y `AzureAgentVersionCreator.cs` (adaptador del SDK con credenciales reales). No se excluyen configuración, especificaciones, catálogo, despliegue ni el formatter de Azure DevOps.

Microsoft.NET.Test.Sdk requiere Microsoft.CodeCoverage como dependencia NuGet. Su referencia con `ExcludeAssets="all"` y `PrivateAssets="all"` desactiva todos sus componentes y evita propagarlos; no se usa para recopilar cobertura.
