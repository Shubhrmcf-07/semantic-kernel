// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Connectors.AzureLogicApps.LogicApps.Entities;
internal class AgenticConnection : IConnection
{
    AADAuth _authentication;
    string _endpoint;
    string _resourceId;

    public AgenticConnection(AADAuth auth, string endpoint, string resourceId)
    {
        this._authentication = auth;
        this._endpoint = endpoint;
        this._resourceId = resourceId;
    }
}

public class AADAuth
{
    public string clientId { get; set; }
    public string secret { get; set; }
    public string tenant { get; set; }
    public string type { get; init; } = "ActiveDirectoryOAuth";
}
