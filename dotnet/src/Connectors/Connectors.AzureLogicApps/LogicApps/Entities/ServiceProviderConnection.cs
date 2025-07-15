// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Entities;
internal class ServiceProviderConnection : IConnection
{
    string parameterSetName { get; set; }
    ParameterValues parameterValues { get; set; }

    ServiceProvider serviceProvider { get; set; }

}

public class ParameterValues
{
    AADAuth authProvider { get; set; }
}


internal class ServiceProvider
{
    string id { get; set; }
}
