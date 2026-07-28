using System.ComponentModel;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AzureAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace MsFoundryAgent;

public static class Program
{
    private const string DefaultAgentName = "Andres-Agent";
    private const string DefaultAgentInstructions =
        "You are an analytical AI agent specialized in reading, understanding, and extracting insights from provided information.";

    public static async Task Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        string projectEndpoint = config["Foundry:ProjectEndpoint"]
            ?? throw new InvalidOperationException("Foundry:ProjectEndpoint is not configured.");

        string modelDeployment = config["Foundry:ModelDeployment"]
            ?? throw new InvalidOperationException("Foundry:ModelDeployment is not configured.");

        string agentName = config["Foundry:AgentName"] ?? DefaultAgentName;
        string agentInstructions = config["Foundry:AgentInstructions"] ?? DefaultAgentInstructions;

        var credential = new DefaultAzureCredential();

        var aiProjectClient = new AIProjectClient(
            new Uri(projectEndpoint),
            credential);

        Console.WriteLine($"Creating agent '{agentName}' on Azure AI Foundry...");

        FoundryAgent agent = await aiProjectClient.CreateAIAgentAsync(
            name: agentName,
            model: modelDeployment,
            instructions: agentInstructions,
            description: null,
            tools: []);

        if (args.Length > 0 && args[0].Equals("deploy", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Agent '{agent.Name}' deployed successfully.");
            Console.WriteLine("The agent was left in Azure AI Foundry and was not deleted.");
            return;
        }

        if (args.Length > 0 && args[0].Equals("verify", StringComparison.OrdinalIgnoreCase))
        {
            FoundryAgent found = await aiProjectClient.GetAIAgentAsync(agentName, tools: []);
            Console.WriteLine("Agent verification succeeded.");
            Console.WriteLine($"Name: {found.Name}");
            Console.WriteLine($"Model: {modelDeployment}");
            Console.WriteLine($"Endpoint: {projectEndpoint}");
            Console.WriteLine("If the portal does not show it, confirm you are in the same Foundry project and tenant.");
            return;
        }

        Console.WriteLine("Agent created. Starting multi-turn conversation (type 'quit' to exit).\n");

        AgentSession session = await agent.CreateSessionAsync();

        while (true)
        {
            Console.Write("You: ");
            string? userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput) ||
                userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.Write("Agent: ");
            await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(
                new ChatMessage(ChatRole.User, userInput), session, null, default))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    Console.Write(update.Text);
                }
            }
            Console.WriteLine("\n");
        }

        Console.WriteLine("Session ended. Agent was not deleted.");
    }
}
