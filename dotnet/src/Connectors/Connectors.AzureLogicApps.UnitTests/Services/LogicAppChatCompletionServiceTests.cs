using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Entities;
using Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Extension;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.UnitTests.Services
{
    public class LogicAppChatCompletionServiceTests
    {
        [Fact]
        public async Task SampleTestAsync()
        {
            var laBuilder = new LogicAppBuilder();
            var workflow = laBuilder.Init("agentic");

            laBuilder.AddConnection(
                "agentic-1",
                LogicApps.Enums.ConnectionType.Agentic,
                "",
                "",
                "",
                new JsonObject
                {
                    ["endpoint"] = "",
                    ["resourceId"] = ""
                });
            workflow.CreateAgent("Test Agent", "agenticConn", "gpt-4.1-mini", "You are a helpful assistant.");
            var bulder = Kernel.CreateBuilder().AddLogicAppChatCompletionService("agentic");
            var kernel = bulder.Build();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            kernel.Plugins.AddFromType<LightsPlugin>();
            var history = new ChatHistory();
            string? userInput = "How much is the conpany spending on employee salaries?";

            // Collect user input
            Console.Write("User > ");
            userInput = userInput;

            // Add user input
            history.AddUserMessage(userInput);

            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                kernel: kernel);

            // Print the results
            Console.WriteLine("Assistant > " + result);

            // Add the message from the agent to the chat history
            history.AddMessage(result.Role, result.Content ?? string.Empty);

            Assert.Fail(result.Content);
        }
    }
}
