// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
#if !ANDROID
    /// <summary>
    /// Server-side peer for browser (WASM) clients over a WebSocket connection
    /// (SqlTunnel-OptionB-Implementation.md §8). Plays the same role as <see cref="WCFService"/>
    /// (the CoreWCF per-session service), but the transport is a plain WebSocket:
    /// one binary WebSocket message = one serialized <see cref="WCFMessage"/>.
    /// A <see cref="WCFServiceChannel"/> is started per connection (same as WCFService does),
    /// so all dispatch/subscription logic is reused unchanged.
    /// </summary>
    public class WCFBrowserService : IWCFServicePeer
    {
        #region private members
        private WebSocket _socket;
        private WCFServiceChannel _acServiceObject = null;
        private Uri _RemoteAddress = null;
        private bool _ClosingConnection = false;
        private bool _stoppingChannel = false;
        // WebSocket allows only one concurrent SendAsync
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private static ACMonitorObject _20061_InitLock = new ACMonitorObject(20061);
        #endregion

        #region c´tors
        public WCFBrowserService(WebSocket socket, string remoteAddress = null)
        {
            _socket = socket;
            if (!String.IsNullOrEmpty(remoteAddress))
            {
                try
                {
                    if (remoteAddress.Contains(':'))
                        _RemoteAddress = new Uri(String.Format("ws://[{0}]/", remoteAddress));
                    else
                        _RemoteAddress = new Uri(String.Format("ws://{0}/", remoteAddress));
                }
                catch (Exception)
                {
                    _RemoteAddress = new Uri("ws://browser/");
                }
            }
            else
                _RemoteAddress = new Uri("ws://browser/");
        }
        #endregion

        #region public members
        /// <summary>
        /// Called by the WebSocket endpoint after the <see cref="WCFServiceChannel"/> was started
        /// for this connection (mirrors WCFService's constructor wiring).
        /// </summary>
        internal void AttachChannel(WCFServiceChannel channel)
        {
            _acServiceObject = channel;
        }

        /// <summary>
        /// Ingress: a frame was received from the browser client and is handed to the channel.
        /// </summary>
        public void Invoke(WCFMessage acMessage)
        {
            if (_acServiceObject == null)
                return;
            _acServiceObject.EnqeueReceivedMessageFromPeer(acMessage);
        }

        /// <summary>
        /// Reads WebSocket frames until the client disconnects. One binary frame = one WCFMessage.
        /// Returns when the connection is closed; the channel is stopped afterwards.
        /// </summary>
        public async Task RunReceiveLoopAsync()
        {
            byte[] buffer = new byte[64 * 1024];
            try
            {
                while (_socket != null && _socket.State == WebSocketState.Open)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                try { await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "client close", CancellationToken.None); } catch (Exception) { }
                                return;
                            }
                            ms.Write(buffer, 0, result.Count);
                        }
                        while (!result.EndOfMessage);

                        WCFMessage acMessage = null;
                        try
                        {
                            acMessage = WCFBrowserSerializer.Deserialize(ms.ToArray());
                        }
                        catch (Exception e)
                        {
                            LogError("RunReceiveLoopAsync(Deserialize)", e);
                        }
                        if (acMessage != null)
                            Invoke(acMessage);
                    }
                }
            }
            catch (Exception e)
            {
                LogError("RunReceiveLoopAsync", e);
            }
            finally
            {
                DisconnectClient();
            }
        }
        #endregion

        #region IWCFServicePeer
        public bool InvokeRemote(WCFMessage acMessage)
        {
            if (_socket == null || _ClosingConnection || _socket.State != WebSocketState.Open)
                return false;
            byte[] frame = null;
            try
            {
                frame = WCFBrowserSerializer.Serialize(acMessage);
            }
            catch (Exception e)
            {
                LogError("InvokeRemote(Serialize)", e);
                DisconnectClient();
                return false;
            }
            try
            {
                _sendLock.Wait();
                try
                {
                    _socket.SendAsync(new ArraySegment<byte>(frame), WebSocketMessageType.Binary, true, CancellationToken.None).GetAwaiter().GetResult();
                }
                finally
                {
                    _sendLock.Release();
                }
            }
            catch (Exception e)
            {
                LogError("InvokeRemote(Send)", e);
                DisconnectClient();
                return false;
            }
            return true;
        }

        public void DisconnectClient()
        {
            if (_ClosingConnection)
                return;
            _ClosingConnection = true;
            try
            {
                if (_socket != null && _socket.State == WebSocketState.Open)
                    _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "server close", CancellationToken.None).GetAwaiter().GetResult();
                else if (_socket != null)
                    _socket.Dispose();
            }
            catch (Exception e)
            {
                LogError("DisconnectClient", e);
            }
            StopChannel();
        }

        public Uri RemoteAddress
        {
            get { return _RemoteAddress; }
        }

        public bool ClosingConnection
        {
            get { return _ClosingConnection; }
            set { _ClosingConnection = value; }
        }
        #endregion

        #region Helper
        private void StopChannel()
        {
            WCFServiceChannel channel = _acServiceObject;
            if (channel == null || _stoppingChannel)
                return;
            _stoppingChannel = true;
            Task.Run(async () =>
            {
                try
                {
                    WCFServiceManager serviceManager = channel.WCFServiceManager;
                    if (serviceManager != null)
                    {
                        if (await serviceManager.StopComponent(channel))
                            _acServiceObject = null;
                    }
                }
                catch (Exception e)
                {
                    LogError("StopChannel", e);
                }
            });
        }

        private void LogError(string method, Exception e)
        {
            string msg = e.Message;
            if (e.InnerException != null && e.InnerException.Message != null)
                msg += " Inner:" + e.InnerException.Message;
            WCFServiceChannel channel = _acServiceObject;
            if (channel != null && channel.Messages != null)
                channel.Messages.LogException("WCFBrowserService", method, msg);
            else if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                datamodel.Database.Root.Messages.LogException("WCFBrowserService", method, msg);
        }
        #endregion
    }
#endif
}
