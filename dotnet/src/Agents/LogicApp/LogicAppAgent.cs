// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.SemanticKernel.Agents.LogicApp;

/// <summary>
/// Azure Logic App Agent.
/// </summary>
internal class LogicAppAgent : Agent
{
    private HttpClient _httpClient = new HttpClient();
    private readonly string _agentWorkflowTriggerUrl;

    public LogicAppAgent(string agentWorkflow)
        : base()
    {
        this.Description = "This agent is designed to interact with Azure Logic Apps for processing requests.";
        var callbackUrlResponse = this._httpClient.PostAsync($"http://localhost:7071/runtime/webhooks/workflow/api/management/workflows/{agentWorkflow}/triggers/When_a_HTTP_request_is_received/listCallbackUrl?api-version=2019-10-01-edge-preview", null).Result;
        var callbackUrlContent = callbackUrlResponse.Content.ReadAsStringAsync().Result;
        var callbackUrlContentJson = JToken.Parse(callbackUrlContent);
        this._agentWorkflowTriggerUrl = callbackUrlContentJson["value"].ToString();
    }

    public override async IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InvokeAsync(ICollection<ChatMessageContent> messages, AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
    {
        HttpContent content = new StringContent("What is the amount of money company is spending on salaries?");
        var response = await this._httpClient.PostAsync(_agentWorkflowTriggerUrl, content)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        yield return new AgentResponseItem<ChatMessageContent>(new ChatMessageContent
        {
            Role = AuthorRole.Assistant,
            Content = responseContent
        }, new LogicAppAgentThread());
    }

    public override IAsyncEnumerable<AgentResponseItem<StreamingChatMessageContent>> InvokeStreamingAsync(ICollection<ChatMessageContent> messages, AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    protected override Task<AgentChannel> CreateChannelAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override IEnumerable<string> GetChannelKeys()
    {
        throw new NotImplementedException();
    }

    protected override Task<AgentChannel> RestoreChannelAsync(string channelState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
