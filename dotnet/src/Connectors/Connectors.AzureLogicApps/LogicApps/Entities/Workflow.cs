// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Entities.Agent;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Entities;
internal class Workflow
{
    private readonly string _workflowJsonPath;
    private readonly string _workflowName;
    private WorkflowSchema _doc;

    internal Workflow(string name, string execSitePath)
    {
        _workflowJsonPath = Path.Combine(execSitePath, name, "workflow.json");
        _workflowName = name;
        _doc = JsonSerializer.Deserialize<WorkflowSchema>(File.ReadAllText(_workflowJsonPath));
    }

    public void CreateAgent(string agentName, string referenceName, string deploymentId, string systemPrompt)
    {
        var key = agentName.Replace(" ", "_");

        var action = new AgentAction
        {
            inputs = new Inputs
            {
                parameters = new Parameters
                {
                    deploymentId = deploymentId,
                    messages = new[]
                    {
                        new AgentMessage(AgentMessageRole.SystemRole, systemPrompt)
                    }
                },
                modelConfigurations = new ModelConfigurations
                {
                    model1 = new ModelRef { referenceName = referenceName }
                }
            }
        };

        _doc.definition.actions[key] = JsonSerializer.SerializeToNode(action)!;
        File.WriteAllText(
        _workflowJsonPath,
        JsonSerializer.Serialize(_doc, new JsonSerializerOptions { WriteIndented = true }));
    }
}
