// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Agents.LogicApp.Extension;
internal class LogicAppBuilderExtension
{
    private static readonly string s_hostJsonContent = @"{
        ""version"": ""2.0"",
        ""logging"": {
        ""applicationInsights"": {
            ""samplingSettings"": {
            ""isEnabled"": true,
            ""excludedTypes"": ""Request""
            }
        }
        },
        ""extensionBundle"": {
        ""id"": ""Microsoft.Azure.Functions.ExtensionBundle.Workflows"",
        ""version"": ""[1.*, 2.0.0)""
        },
        ""extensions"": {
        ""workflow"": {
            ""Settings"": {
            ""Runtime.AllowAgenticKind"": ""true""
            }
        }
        }
    }";

    private static readonly string s_localSettingsJsonContent = @"{
      ""IsEncrypted"": false,
      ""Values"": {
        ""AzureWebJobsStorage"": ""UseDevelopmentStorage=true"",
        ""FUNCTIONS_WORKER_RUNTIME"": ""node"",
        ""APP_KIND"": ""workflowapp"",
        ""ProjectDirectoryPath"": ""{projectPath}"",
        ""WORKFLOWS_SUBSCRIPTION_ID"": """",
        ""agent_openAIEndpoint"": ""https://siddharth-ai-service.cognitiveservices.azure.com/"",
        ""agent_openAIKey"": ""3wNLdePKfF8oX1pimfhpTj549ayjmE5FXhtzYMjOEhvwIlTbLDn0JQQJ99BDACL93NaXJ3w3AAAAACOGFUQb""
      }
    }";

    public static void ConfigureLogicAppAgent(string connectionJson, string workflowName, string workflowJson)
    {
        var processes = Process.GetProcessesByName("func");
        foreach (var process in processes)
        {
            process.Kill();
        }

        var tempPath = Path.GetTempPath() + Guid.NewGuid();
        var connectionFilePath = Path.Combine(tempPath, "connections.json");
        var workflowFilePath = Path.Combine(tempPath, workflowName, "workflow.json");
        Directory.CreateDirectory(Path.GetDirectoryName(workflowFilePath));
        var hostJsonFilePath = Path.Combine(tempPath, "host.json");
        var localSettingsJsonFilePath = Path.Combine(tempPath, "local.settings.json");

        File.WriteAllText(connectionFilePath, connectionJson);
        File.WriteAllText(workflowFilePath, workflowJson);
        File.WriteAllText(hostJsonFilePath, s_hostJsonContent);
        File.WriteAllText(localSettingsJsonFilePath, s_localSettingsJsonContent.Replace("{projectPath}", tempPath.Replace("\\", "\\\\")));

        var processStarted = new TaskCompletionSource<bool>();
        using Process logicAppProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "func",
                Arguments = $"host start --port 7071 --verbose",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = tempPath,
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
}
