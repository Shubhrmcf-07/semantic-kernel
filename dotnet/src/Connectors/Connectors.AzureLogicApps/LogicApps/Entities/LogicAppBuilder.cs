// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Enums;
using Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Extension;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Entities;
internal class LogicAppBuilder
{
    public string tempPath;
    public string execSiteTemplatePath;
    public JsonSerializerOptions options => new() { WriteIndented = true };

    public LogicAppBuilder()
    {
        tempPath = Path.GetTempPath() + Guid.NewGuid();
        var processes = Process.GetProcessesByName("func");
        foreach (var process in processes)
        {
            process.Kill();
        }
        Directory.CreateDirectory(tempPath);
        var connectionFilePath = Path.Combine(tempPath, "connections.json");
        var hostJsonFilePath = Path.Combine(tempPath, "host.json");
        var localSettingsJsonFilePath = Path.Combine(tempPath, "local.settings.json");
        var parametersFilePath = Path.Combine(tempPath, "parameters.json");

        var implBase = Path.GetDirectoryName(typeof(LogicAppBuilderExtension).Assembly.Location)!;
        execSiteTemplatePath = Path.Combine(implBase, "Templates");

        File.Copy(Path.Combine(execSiteTemplatePath, "connections.json"), connectionFilePath);
        File.Copy(Path.Combine(execSiteTemplatePath, "host.json"), hostJsonFilePath);
        File.Copy(Path.Combine(execSiteTemplatePath, "local.settings.json"), localSettingsJsonFilePath);
        File.Copy(Path.Combine(execSiteTemplatePath, "parameters.json"), parametersFilePath);

        var processStarted = new TaskCompletionSource<bool>();
        using Process logicAppProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c func host start --port 7071 --verbose",
                WorkingDirectory = tempPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        logicAppProcess.Start();
        logicAppProcess.BeginOutputReadLine();
        logicAppProcess.BeginErrorReadLine();

        logicAppProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.WriteLine(e.Data);
            }
        };
        logicAppProcess.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null && e.Data.Contains("Host started"))
            {
                processStarted.TrySetResult(true);
            }
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.WriteLine(e.Data);
            }
        };

        var result = Task.WhenAny(processStarted.Task, Task.Delay(TimeSpan.FromSeconds(30))).Result;

        if (result != processStarted.Task)
        {
            throw new InvalidOperationException("runtime did not start properly");
        }

        if (logicAppProcess.HasExited)
        {
            throw new InvalidOperationException($"runtime did not start properly");
        }

    }

    public Workflow Init(string flowName)
    {
        var wfFolder = Path.Combine(tempPath, flowName);
        Directory.CreateDirectory(wfFolder);

        var wfPath = Path.Combine(wfFolder, "workflow.json");

        var doc = new WorkflowSchema();

        doc.definition.triggers["When_a_HTTP_request_is_received"] = new
        {
            type = "Request",
            kind = "Http"
        };

        File.WriteAllText(wfPath, JsonSerializer.Serialize(doc, options));

        return new Workflow(flowName, tempPath);
    }

    public void AddConnection(string connectionName, ConnectionType connectionType, string clientId, string clientSecret, string tenantId, JsonObject? additionalParameters)
    {
        var connectionsJsonFilePath = Path.Combine(tempPath, "connections.json");
        var root = JsonNode.Parse(File.ReadAllText(connectionsJsonFilePath));

        string bucketName = connectionType switch
        {
            ConnectionType.Agentic => "agentConnections",
            ConnectionType.ServiceProvider => "serviceProviderConnections",
            ConnectionType.ManagedApi => "managedApiConnections",
            _ => throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null)
        };

        var auth = new AADAuth
        {
            clientId = clientId,
            secret = clientSecret,
            tenant = tenantId,
            type = "ActiveDirectoryOAuth"
        };

        var finalObj = new JsonObject
        {
            ["authentication"] = JsonSerializer.SerializeToNode(auth)!
        };

        foreach (var kvp in additionalParameters)
        {
            finalObj[kvp.Key] = kvp.Value?.DeepClone();
        }


        if (connectionType == ConnectionType.Agentic)
        {
            finalObj["type"] = "FoundryAgentService";
        }

        var bucket = root[bucketName] as JsonObject ?? new JsonObject();
        bucket[connectionName] = finalObj;
        root[bucketName] = bucket;
        File.WriteAllText(connectionsJsonFilePath,
            JsonSerializer.Serialize(root, this.options));
    }
}
