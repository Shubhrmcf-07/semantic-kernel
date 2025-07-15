// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json.Linq;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Services;

internal class LogicAppChatCompletionService : IChatCompletionService
{
    public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

    private HttpClient _httpClient = new HttpClient();
    private readonly string _agentWorkflowTriggerUrl;

    public LogicAppChatCompletionService(string agentWorkflow)
        : base()
    {
        var callbackUrlResponse = this._httpClient.PostAsync($"http://localhost:7071/runtime/webhooks/workflow/api/management/workflows/{agentWorkflow}/triggers/When_a_HTTP_request_is_received/listCallbackUrl?api-version=2019-10-01-edge-preview", null).Result;
        var callbackUrlContent = callbackUrlResponse.Content.ReadAsStringAsync().Result;
        var callbackUrlContentJson = JToken.Parse(callbackUrlContent);
        this._agentWorkflowTriggerUrl = callbackUrlContentJson["value"].ToString();
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        HttpContent content = new StringContent(chatHistory.Last().Content);
        var response = await this._httpClient.PostAsync(_agentWorkflowTriggerUrl, content)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return new List<ChatMessageContent>
        {
            {
                new ChatMessageContent
                {
                    Role = AuthorRole.Assistant,
                    Content = responseContent
                }
            }
        };
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
