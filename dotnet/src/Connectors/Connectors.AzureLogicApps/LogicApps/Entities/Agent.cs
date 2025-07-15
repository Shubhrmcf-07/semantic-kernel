// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Entities;
internal class Agent
{
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentMessageRole
{
    SystemRole,
    UserRole,
    ToolRole
}

public sealed record AgentMessage(AgentMessageRole role, string content);

public sealed class AgentAction : IAction
{
    public string type { get; set; } = "Agent";
    public IInputs inputs { get; set; } = new AgentInputs();
    public JsonObject runAfter { get; set; } = new();

    public JsonObject tools { get; set; } = new();
    public Limit limit { get; set; } = new();
}

public sealed class Parameters
{
    public string agentModelType { get; init; } = "FoundryAgentService";
    public string deploymentId { get; set; } = "";
    public AgentMessage[] messages { get; set; } = System.Array.Empty<AgentMessage>();
    public AgentModelSettings agentModelSettings { get; init; } = new();
}

public sealed class AgentModelSettings
{
    public AgentChatCompletionSettings agentChatCompletionSettings { get; init; } = new();
}

public sealed class AgentChatCompletionSettings
{
    public double temperature { get; init; } = 0.1;
    public double topP { get; init; } = 0.1;
}

public sealed class ModelConfigurations
{
    public ModelRef model1 { get; init; } = new();
}

public sealed class ModelRef
{
    public string referenceName { get; set; } = "";
}

public sealed class Limit
{
    public int count { get; init; } = 10;
    public string timeout { get; init; } = "PT1H";
}

public class AgentInputs : IInputs
{
    public Parameters parameters { get; set; } = new();
    public ModelConfigurations modelConfigurations { get; init; } = new();
}
