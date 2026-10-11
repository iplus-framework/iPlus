// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Transport-agnostic peer of a <see cref="WCFServiceChannel"/>. Implemented by
    /// <see cref="WCFService"/> (WCF duplex, net.tcp/http) and by <see cref="WCFBrowserService"/>
    /// (WebSocket endpoint for browser/WASM clients, see SqlTunnel-OptionB-Implementation.md §8).
    /// </summary>
    public interface IWCFServicePeer
    {
        /// <summary>Pushes a message from the server to the connected client. Returns false if the connection is broken.</summary>
        bool InvokeRemote(WCFMessage acMessage);

        /// <summary>Closes the connection to the client.</summary>
        void DisconnectClient();

        /// <summary>Remote address of the connected client (diagnostics).</summary>
        Uri RemoteAddress { get; }

        /// <summary>True once the connection is closing/closed and the channel should be stopped.</summary>
        bool ClosingConnection { get; set; }
    }
}
