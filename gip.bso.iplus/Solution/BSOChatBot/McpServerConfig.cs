// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json.Serialization;

namespace gip.bso.iplus
{
    public class McpServerConfig
    {
        public Dictionary<string, McpServerInfo> mcpServers { get; set; }
    }

    public class McpServerInfo
    {
        public bool? ForLocalBotUsage { get; set; }
        public string command { get; set; }
        public string[] args { get; set; }
        public Dictionary<string, string> env { get; set; }
    }
}