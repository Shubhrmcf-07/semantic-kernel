// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Agents.LogicApp;
internal class LogicAppAgentThread : AgentThread
{
    protected override Task<string?> CreateInternalAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task DeleteInternalAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task OnNewMessageInternalAsync(ChatMessageContent newMessage, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
