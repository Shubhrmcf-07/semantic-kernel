// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Entities;
internal class WorkflowSchema
{
    public Definition definition { get; init; } = new();
    public string kind { get; init; } = "Agentic";
}

public sealed class Definition
{
    [JsonPropertyName("$schema")]
    public string Schema { get; init; } = "https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#";

    public string contentVersion { get; init; } = "1.0.0.0";

    public Dictionary<string, object> triggers { get; init; } = new();
    public Dictionary<string, JsonNode> actions { get; init; } = new();
    public Dictionary<string, object> outputs { get; init; } = new();
}
