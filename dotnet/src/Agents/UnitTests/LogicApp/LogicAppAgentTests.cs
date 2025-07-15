// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Agents.LogicApp;
using Microsoft.SemanticKernel.Agents.LogicApp.Extension;
using Xunit;

namespace SemanticKernel.Agents.UnitTests.LogicApp;

/// <summary>
/// <see cref="LogicAppAgent"/>
/// </summary>
public class LogicAppAgentTests
{
    [Fact]
    public async Task CreateClient_WithValidSettings_ReturnsConfiguredClient()
    {
        LogicAppBuilderExtension.ConfigureLogicAppAgent(
            "{\n  \"agentConnections\": {\n    \"agent\": {\n      \"displayName\": \"new_conn_e3820\",\n      \"authentication\": {\n        \"type\": \"Key\",\n        \"key\": \"@appsetting('agent_openAIKey')\"\n      },\n      \"endpoint\": \"@appsetting('agent_openAIEndpoint')\",\n      \"resourceId\": \"/subscriptions/f34b22a3-2202-4fb1-b040-1332bd928c84/resourceGroups/siddharth-imp/providers/Microsoft.CognitiveServices/accounts/siddharth-ai-service\",\n      \"type\": \"model\"\n    }\n  },\n  \"managedApiConnections\": {}\n}\n",
            "agentic-2",
            "{\n    \"definition\": {\n        \"$schema\": \"https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#\",\n        \"contentVersion\": \"1.0.0.0\",\n        \"actions\": {\n            \"Default_Agent\": {\n                \"type\": \"Agent\",\n                \"inputs\": {\n                    \"parameters\": {\n                        \"agentModelType\": \"AzureOpenAI\",\n                        \"deploymentId\": \"gpt-4o-4\",\n                        \"messages\": [\n                            {\n                                \"role\": \"system\",\n                                \"content\": \"You are a company with users\"\n                            },\n                            {\n                                \"role\": \"user\",\n                                \"content\": \"@{triggerBody()}\"\n                            }\n                        ],\n                        \"agentModelSettings\": {\n                            \"agentHistoryReductionSettings\": {\n                                \"agentHistoryReductionType\": \"maximumTokenCountReduction\",\n                                \"maximumTokenCount\": 128000\n                            },\n                            \"deploymentModelProperties\": {\n                                \"name\": \"gpt-4.1\",\n                                \"format\": \"OpenAI\",\n                                \"version\": \"2025-04-14\"\n                            }\n                        }\n                    },\n                    \"modelConfigurations\": {\n                        \"model1\": {\n                            \"referenceName\": \"agent\"\n                        }\n                    }\n                },\n                \"tools\": {\n                    \"Tool\": {\n                        \"actions\": {\n                            \"Compose\": {\n                                \"type\": \"Compose\",\n                                \"inputs\": 10\n                            }\n                        },\n                        \"description\": \"Gets Number Of Users In Company\"\n                    },\n                    \"Tool_2\": {\n                        \"actions\": {\n                            \"Compose_1\": {\n                                \"type\": \"Compose\",\n                                \"inputs\": 10000\n                            }\n                        },\n                        \"description\": \"Gets the salary of single user\"\n                    }\n                },\n                \"runAfter\": {},\n                \"limit\": {\n                    \"count\": 100\n                }\n            },\n            \"Response\": {\n                \"type\": \"Response\",\n                \"kind\": \"Http\",\n                \"inputs\": {\n                    \"statusCode\": 200,\n                    \"body\": \"@outputs('Default_Agent')?['lastAssistantMessage']\"\n                },\n                \"runAfter\": {\n                    \"Default_Agent\": [\n                        \"SUCCEEDED\"\n                    ]\n                }\n            }\n        },\n        \"outputs\": {},\n        \"triggers\": {\n            \"When_a_HTTP_request_is_received\": {\n                \"type\": \"Request\",\n                \"kind\": \"Http\"\n            }\n        }\n    },\n    \"kind\": \"Agentic\"\n}");

        LogicAppAgent logicAppAgent = new LogicAppAgent("agentic-2");
        var result = logicAppAgent.InvokeAsync();
        await foreach (var message in result)
        {
            Assert.Fail(message.Message.Content);
        }
    }
}
