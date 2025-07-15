// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Services;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Extension;
public static class LogicAppKernelBuilderExtension
{
    public static IKernelBuilder AddLogicAppChatCompletionService(
        this IKernelBuilder builder,
        string flowName,
        string? serviceId = null)
    {
        Verify.NotNull(builder);

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) =>
        {
            try
            {
                return new LogicAppChatCompletionService(flowName);
            }
            catch (Exception ex)
            {
                throw new KernelException($"An error occurred while initializing the {nameof(LogicAppChatCompletionService)}: {ex.Message}", ex);
            }
        });

        return builder;
    }
}
