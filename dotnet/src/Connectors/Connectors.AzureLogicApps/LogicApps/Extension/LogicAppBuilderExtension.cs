// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Extension;
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
        ""agent_openAIEndpoint"": """",
        ""agent_openAIKey"": """"
      }
    }";

    public static void CreateLogicAppBuilder(string workflowName)
    {
        var processes = Process.GetProcessesByName("func");
        foreach (var process in processes)
        {
            process.Kill();
        }
        var tempPath = Path.GetTempPath() + Guid.NewGuid();
        Directory.CreateDirectory(tempPath);
        var connectionFilePath = Path.Combine(tempPath, "connections.json");
        var workflowFilePath = Path.Combine(tempPath, workflowName, "workflow.json");
        var hostJsonFilePath = Path.Combine(tempPath, "host.json");
        var localSettingsJsonFilePath = Path.Combine(tempPath, "local.settings.json");
        var parametersFilePath = Path.Combine(tempPath, "parameters.json");
        Directory.CreateDirectory(Path.GetDirectoryName(workflowFilePath));

        var implBase = Path.GetDirectoryName(typeof(LogicAppBuilderExtension).Assembly.Location)!;
        var templateRoot = Path.Combine(implBase, "Templates");

        File.Copy(Path.Combine(templateRoot, "connections.json"), connectionFilePath);
        File.Copy(Path.Combine(templateRoot, "workflow.json"), workflowFilePath);
        File.Copy(Path.Combine(templateRoot, "host.json"), hostJsonFilePath);
        File.Copy(Path.Combine(templateRoot, "local.settings.json"), localSettingsJsonFilePath);
        File.Copy(Path.Combine(templateRoot, "parameters.json"), parametersFilePath);

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
