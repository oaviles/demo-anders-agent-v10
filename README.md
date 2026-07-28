# Simple .NET Agent on Azure AI Foundry

A minimal C# console application that creates and runs an AI agent hosted in **Azure AI Foundry** using the [Microsoft Agent Framework](https://github.com/microsoft/agents).

## Features

| Feature | Details |
|---|---|
| Agent hosting | Azure AI Foundry (server-side agent) |
| Auth | `DefaultAzureCredential` (passwordless) |
| Multi-turn chat | Local message history passed across turns |
| Streaming | `RunStreamingAsync` for real-time output |
| Deployment mode | `dotnet run -- deploy` leaves the agent in Foundry |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- An **Azure AI Foundry** project with a model deployment (e.g. `gpt-4.1`)
- Azure CLI (`az login`) **or** any credential supported by `DefaultAzureCredential`

## Quick Start

### 1. Configure your endpoint

Edit [appsettings.json](appsettings.json):

```json
{
  "Foundry": {
    "ProjectEndpoint": "https://<your-hub>.services.ai.azure.com/api/projects/<your-project>",
    "ModelDeployment": "gpt-4.1"
  }
}
```

You can find your **Project Endpoint** in the Azure AI Foundry portal under  
**Project → Overview → Project details → API endpoint**.

Alternatively, set environment variables to override `appsettings.json`:

```powershell
$env:Foundry__ProjectEndpoint = "https://..."
$env:Foundry__ModelDeployment = "gpt-4.1"
$env:Foundry__AgentName = "AndersAgent"
```

### 2. Authenticate

```powershell
az login

az login --use-device-code

```

`DefaultAzureCredential` will use your `az login` session locally and Managed Identity in Azure.

### 3. Run

```powershell
cd ms_foundry_agent
dotnet run
```

Deploy-only mode:

```powershell
dotnet run -- deploy
```

Example session:

```
Creating agent 'SimpleFoundryAgent' on Azure AI Foundry...
Agent created. Starting multi-turn conversation (type 'quit' to exit).

You: Hello, how can you help me?
Agent: I'm an analytical AI agent specialized in reading, understanding, and extracting insights from provided information. I can help you analyze data and information.

You: Can you summarize this text for me?
Agent: Of course! Please provide the text you'd like me to summarize.

You: quit
Cleaning up agent...
Done.
```

## Project Structure

```
ms_foundry_agent/
├── ms_foundry_agent.csproj   # NuGet references
├── Program.cs                 # Agent creation, tool, and conversation loop
├── appsettings.json           # Foundry endpoint & model deployment name
└── README.md
```

## Key Packages

| Package | Purpose |
|---|---|
| `Azure.AI.Projects` | `AIProjectClient` — create/delete Foundry agents |
| `Azure.Identity` | `DefaultAzureCredential` — passwordless auth |
| `Microsoft.Agents.AI.AzureAI` | `ChatClientAgent`, streaming, Foundry integration |
| `Microsoft.Extensions.AI` | `AIFunctionFactory` — function/tool registration |

## Extending the Agent

### Add a new tool

```csharp
[Description("Get the stock price for a ticker symbol.")]
public static decimal GetStockPrice(
    [Description("Stock ticker symbol, e.g. 'MSFT'")] string ticker)
{
    // call your real API here
    return 420.42m;
}

// Register it when creating the agent:
tools: [AIFunctionFactory.Create(GetStockPrice)]
```

## Bash Deployment Script

The repository includes [deploy-foundry-agent.sh](c:/Users/edaviles/Code/MultiAgent-Demo/ms_foundry_agent/deploy-foundry-agent.sh) for Git Bash, WSL, or Cloud Shell.

Required environment variables:

```bash
export FOUNDRY_PROJECT_ENDPOINT="https://<resource>.services.ai.azure.com/api/projects/<project-name>"
export FOUNDRY_MODEL_DEPLOYMENT_NAME="gpt-4.1"
```

Optional environment variables:

```bash
export FOUNDRY_AGENT_NAME="SimpleFoundryAgent"
export FOUNDRY_AGENT_INSTRUCTIONS="You are an analytical AI agent specialized in reading, understanding, and extracting insights from provided information."
```

Run the deployment:

```bash
./deploy-foundry-agent.sh
```

The script:

1. Verifies `az` and `dotnet` are installed.
2. Ensures you're signed in with `az login`.
3. Maps the bash environment variables into the app's .NET configuration keys.
4. Calls `dotnet run -- deploy` to create the agent in Azure AI Foundry and leave it deployed.

## PowerShell Deployment Script

The repository includes [deploy-foundry-agent.ps1](deploy-foundry-agent.ps1) for native PowerShell deployment on Windows.

Using environment variables:

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<resource>.services.ai.azure.com/api/projects/<project-name>"
$env:FOUNDRY_MODEL_DEPLOYMENT_NAME = "gpt-4.1"
./deploy-foundry-agent.ps1
```

Using explicit parameters:

```powershell
./deploy-foundry-agent.ps1 `
  -FoundryProjectEndpoint "https://<resource>.services.ai.azure.com/api/projects/<project-name>" `
  -FoundryModelDeploymentName "gpt-4.1" `
  -FoundryAgentName "SimpleFoundryAgent"
```

Optional parameters:

1. `-FoundryAgentName`
2. `-FoundryAgentInstructions`
3. `-ProjectFile` (defaults to `./ms_foundry_agent.csproj`)

## Deploying to Azure

To run the app itself in Azure (for example on Azure Container Apps or Azure App Service):

1. Build and publish:
   ```powershell
   dotnet publish -c Release -o ./publish
   ```
2. Containerise with a `Dockerfile` or use `azd` for a full IaC deploy.
3. Assign a **Managed Identity** to your Azure resource and grant it the  
  **Azure AI Developer** role on your AI Foundry project — no secrets needed.
