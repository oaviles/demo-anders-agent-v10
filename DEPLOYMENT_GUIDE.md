# Azure AI Foundry Agent Deployment Guide

This guide shows how to deploy the `SimpleFoundryAgent` to Azure AI Foundry using either:

1. PowerShell script: `deploy-foundry-agent.ps1`
2. Bash script: `deploy-foundry-agent.sh`

## Prerequisites

1. Azure subscription with access to your Azure AI Foundry project.
2. `.NET 8 SDK` installed.
3. `Azure CLI` installed.
4. Signed in to Azure CLI:

```powershell
az login

az login --use-device-code
```

5. A valid Azure AI Foundry project endpoint, for example:

```text
https://<resource>.services.ai.azure.com/api/projects/<project-name>
```

6. A model deployment name available in that project, for example: `gpt-4.1`.

## Option 1: Deploy with PowerShell

Script file:

```text
deploy-foundry-agent.ps1
```

### A. Deploy using environment variables

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<resource>.services.ai.azure.com/api/projects/<project-name>"
$env:FOUNDRY_MODEL_DEPLOYMENT_NAME = "gpt-4.1"
$env:FOUNDRY_AGENT_NAME = "SimpleFoundryAgent"
$env:FOUNDRY_AGENT_INSTRUCTIONS = "You are an analytical AI agent specialized in reading, understanding, and extracting insights from provided information."

./deploy-foundry-agent.ps1
```

### B. Deploy using script parameters

```powershell
./deploy-foundry-agent.ps1 `
  -FoundryProjectEndpoint "https://<resource>.services.ai.azure.com/api/projects/<project-name>" `
  -FoundryModelDeploymentName "gpt-4.1" `
  -FoundryAgentName "SimpleFoundryAgent" `
  -FoundryAgentInstructions "You are an analytical AI agent specialized in reading, understanding, and extracting insights from provided information."
```

## Option 2: Deploy with Bash

Script file:

```text
deploy-foundry-agent.sh
```

### A. Deploy from Git Bash / WSL / Cloud Shell

```bash
export FOUNDRY_PROJECT_ENDPOINT="https://<resource>.services.ai.azure.com/api/projects/<project-name>"
export FOUNDRY_MODEL_DEPLOYMENT_NAME="gpt-4.1"
export FOUNDRY_AGENT_NAME="SimpleFoundryAgent"
export FOUNDRY_AGENT_INSTRUCTIONS="You are an analytical AI agent specialized in reading, understanding, and extracting insights from provided information."

./deploy-foundry-agent.sh
```

### B. Run Bash script from PowerShell using Git Bash (Windows)

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<resource>.services.ai.azure.com/api/projects/<project-name>"
$env:FOUNDRY_MODEL_DEPLOYMENT_NAME = "gpt-4.1"

& "C:\Program Files\Git\bin\bash.exe" -lc 'cd "/c/Users/<your-user>/Code/MultiAgent-Demo/ms_foundry_agent" && ./deploy-foundry-agent.sh'
```

## Expected successful output

You should see output similar to:

```text
Deploying agent 'SimpleFoundryAgent' to Azure AI Foundry...
Creating agent 'SimpleFoundryAgent' on Azure AI Foundry...
Agent 'SimpleFoundryAgent' deployed successfully.
The agent was left in Azure AI Foundry and was not deleted.
```

## Verify the deployed agent from code

Run:

```powershell
dotnet run -- verify
```

If verification succeeds, the SDK confirms the agent exists in the configured project.

## Troubleshooting

1. `Missing required command: dotnet` in Bash:
Use Git Bash on Windows (`C:\Program Files\Git\bin\bash.exe`) or add .NET to the Bash PATH.

2. Agent not visible in portal but verification succeeds:
Check you are in the same Foundry project and tenant/account as your deployment endpoint.

3. Authentication errors:
Run `az login` again and confirm subscription context with:

```powershell
az account show
```

## Notes

1. `dotnet run -- deploy` creates the agent and does not delete it.
2. `dotnet run` (interactive mode) creates an agent for the session and deletes it when exiting.
