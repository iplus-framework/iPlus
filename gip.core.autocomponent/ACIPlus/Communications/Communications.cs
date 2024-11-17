// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Timers;
using System.Net;
using gip.core.datamodel;
using System.Diagnostics;
using System.Net.NetworkInformation;


namespace gip.core.autocomponent
{
    /// <summary>
    /// Stellt den VBOberverService mit der 
    /// Funktion Run zur verfügung.
    /// 
    /// Auf die Funktionen des Service kann,
    /// sobald dieser gestartet wurde mit der Klasse
    /// VBObserverService zugegriffen werden.
    /// </summary>
    /// 
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Communication-Manager'}de{'Kommunikations-Manager'}", Global.ACKinds.TACCommunications, Global.ACStorableTypes.Required, false, false)]
    public class Communications : ACComponent
    {
        #region Properties
        WCFServiceManager _WCFServiceManager = null;
        WCFClientManager _WCFClientManager = null;
        #endregion

        #region c'tors
        public Communications(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            return result;
        }

        public void InitServiceHost()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return;
            if (this.Root.HasACModelServer)
            {
                Console.WriteLine("Initializing Service...");

                IPAddress loopbackIPV4 = IPAddress.Parse("127.0.0.1");
                IPAddress loopbackIPV6 = IPAddress.Parse("0:0:0:0:0:0:0:1");

                string hostname = Dns.GetHostName();
                if (String.IsNullOrEmpty(hostname))
                {
                    IPHostEntry host = Dns.GetHostEntry(loopbackIPV4);
                    hostname = host?.HostName; // FQDN
                }
                IPAddress[] ips = Dns.GetHostEntry(hostname).AddressList;
                try
                {
                    bool changed = false;
                    IPAddress firstIPV4 = null;
                    // If automatic discovering of interface is allowed. Else fixed IP-Address is used how user has setup in BSOiPlusNetConfig
                    if (Root.Environment.UserInstance.ServiceObserverEnabledTCP)
                    {
                        if (!string.IsNullOrEmpty(this.Root.Environment.UserInstance.ServerIPV4))
                        {
                            IPAddress serverIPV4Predefined = IPAddress.Parse(this.Root.Environment.UserInstance.ServerIPV4);
                            firstIPV4 = ips.Where(c => c.AddressFamily == AddressFamily.InterNetwork && c.ToString() == serverIPV4Predefined.ToString() && c.ToString() != loopbackIPV4.ToString()).FirstOrDefault();
                        }
                        if (firstIPV4 == null)
                            firstIPV4 = ips.Where(c => c.AddressFamily == AddressFamily.InterNetwork && c != loopbackIPV4).FirstOrDefault();
                        string ip4 = firstIPV4 != null ? firstIPV4.ToString() : "";
                        Messages.LogDebug(this.GetACUrl(), "Communications.InitServiceHost()", "ServerIPV4: " + ip4);
                        if (this.Root.Environment.UserInstance.ServerIPV4 != ip4
                            && !(this.Root.Environment.UserInstance.IsUserDefined && this.Root.Environment.UserInstance.ServerIPV4 == loopbackIPV4.ToString())) // For WSL-Testing only loopbackadpater can be used for connection, therefore don't change IP
                        {
                            this.Root.Environment.UserInstance.ServerIPV4 = ip4;
                            changed = true;
                        }

                        IPAddress firstIPV6 = null;
                        if (!string.IsNullOrEmpty(this.Root.Environment.UserInstance.ServerIPV6))
                        {
                            IPAddress serverIPV6Predefined = IPAddress.Parse(this.Root.Environment.UserInstance.ServerIPV6);
                            firstIPV6 = ips.Where(c => c.AddressFamily == AddressFamily.InterNetworkV6
                                                        && GetIPV6WithoutInterface(c.ToString()) == GetIPV6WithoutInterface(serverIPV6Predefined.ToString())
                                                        && GetIPV6WithoutInterface(c.ToString()) != GetIPV6WithoutInterface(loopbackIPV6.ToString())
                                                        && c.ScopeId == 0) // ScopeId == 0: Public address
                                                            .FirstOrDefault();
                        }
                        if (firstIPV6 == null)
                        {
                            firstIPV6 = ips.Where(c => c.AddressFamily == AddressFamily.InterNetworkV6 && c != loopbackIPV6 && c.ScopeId == 0).FirstOrDefault();
                            // If Public address not found:
                            if (firstIPV6 == null)
                                firstIPV6 = ips.Where(c => c.AddressFamily == AddressFamily.InterNetworkV6 && c != loopbackIPV6).FirstOrDefault();
                        }
                        string ip6 = firstIPV6 != null ? GetIPV6WithoutInterface(firstIPV6.ToString()) : "";
                        Messages.LogDebug(this.GetACUrl(), "Communications.InitServiceHost()", "ServerIPV6: " + ip6);
                        if (this.Root.Environment.UserInstance.ServerIPV6 != ip6
                            && !(this.Root.Environment.UserInstance.IsUserDefined && GetIPV6WithoutInterface(this.Root.Environment.UserInstance.ServerIPV6) == GetIPV6WithoutInterface(loopbackIPV6.ToString()))) // For WSL-Testing only loopbackadpater can be used for connection, therefore don't change IP
                        {
                            this.Root.Environment.UserInstance.ServerIPV6 = ip6;
                            changed = true;
                        }

                        if (this.Root.Environment.UserInstance.UseIPV6 && firstIPV6 == null && firstIPV4 != null)
                        {
                            this.Root.Environment.UserInstance.UseIPV6 = false;
                            changed = true;
                        }
                        else if (!this.Root.Environment.UserInstance.UseIPV6 && firstIPV6 != null && firstIPV4 == null)
                        {
                            this.Root.Environment.UserInstance.UseIPV6 = true;
                            changed = true;
                        }
                        if (firstIPV4 == null && firstIPV6 == null)
                        {
                            firstIPV4 = ips.Where(c => c.AddressFamily == AddressFamily.InterNetwork && c.ToString() == loopbackIPV4.ToString()).FirstOrDefault();
                            ip4 = firstIPV4 != null ? firstIPV4.ToString() : "";
                            Messages.LogDebug(this.GetACUrl(), "Communications.InitServiceHost()", "ServerIPV4 loopback: " + ip4);
                            if (this.Root.Environment.UserInstance.ServerIPV4 != ip4
                                && !(this.Root.Environment.UserInstance.IsUserDefined && this.Root.Environment.UserInstance.ServerIPV4 == loopbackIPV4.ToString()))
                            {
                                this.Root.Environment.UserInstance.UseIPV6 = false;
                                this.Root.Environment.UserInstance.ServerIPV4 = ip4;
                                changed = true;
                            }
                        }
                    }

                    if (this.Root.Environment.UserInstance.Hostname != hostname)
                    {
                        Messages.LogDebug(this.GetACUrl(), "Communications.InitServiceHost()", "Hostname: " + hostname);
                        this.Root.Environment.UserInstance.Hostname = hostname;
                        changed = true;
                    }
                    if (changed)
                        this.Root.Environment.Database.ACSaveChanges();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("Communications", "InitServiceHost", msg);
                }

                // Starte Dienst
                _WCFServiceManager = ACUrlCommand("?WCFServiceManager") as WCFServiceManager;
                if (_WCFServiceManager == null)
                    _WCFServiceManager = (WCFServiceManager)StartComponent("WCFServiceManager", null, new object[] { });
            }

            // Starte Client-Manager
            _WCFClientManager = ACUrlCommand("?WCFClientManager") as WCFClientManager;
            if (_WCFClientManager == null)
                _WCFClientManager = (WCFClientManager)StartComponent("WCFClientManager", null, new object[] { });
        }

        public void StartServiceHost()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return;
            if (_WCFServiceManager != null)
                _WCFServiceManager.OpenServiceHost();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            _WCFServiceManager = null;
            _WCFClientManager = null;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region public Member
        public override bool IsPoolable
        {
            get
            {
                return false;
            }
        }
        //Darf nicht als ACProperty definiert werden, wenn es als Unterobjekt laut Anwendungsdefinition vorhanden ist
        //[ACPropertyCurrent(9999, "WCFServiceManager")] 
        public WCFServiceManager WCFServiceManager
        {
            get
            {
                return _WCFServiceManager;
            }
        }

        //Darf nicht als ACProperty definiert werden, wenn es als Unterobjekt laut Anwendungsdefinition vorhanden ist
        //[ACPropertyCurrent(9999, "WCFClientManager")]
        public WCFClientManager WCFClientManager
        {
            get
            {
                return _WCFClientManager;
            }
        }
        #endregion

        #region Client-Side-Send-Methods

        /// <summary>Sendet eine Clientseitige Nachricht an den Server</summary>
        /// <param name="acMessage"></param>
        /// <param name="forACComponent"></param>
        /// <exception cref="gip.core.autocomponent.ACWCFException">Thrown when disconnected</exception>
        public void SendACMessageToServer(WCFMessage acMessage, IACComponent forACComponent)
        {
            if (_WCFClientManager == null || (ACOperationMode != ACOperationModes.Live))
                return;
            _WCFClientManager.SendACMessageToServer(acMessage, forACComponent);
        }

        /// <summary>Method sends a PropertyValueEvent from this Client/Proxy-Object
        /// to the Real Object on Server-side</summary>
        /// <param name="eventArgs"></param>
        /// <param name="forACComponent"></param>
        public void SendPropertyValueToServer(IACPropertyNetValueEvent eventArgs, IACComponent forACComponent)
        {
            if (_WCFClientManager == null || (ACOperationMode != ACOperationModes.Live))
                return;
            _WCFClientManager.SendPropertyValueToServer(eventArgs, forACComponent);
        }

        /// <summary>
        /// Method subscribes an new generated ACObject for retrieving ValueEvents from the Server
        /// </summary>
        /// <param name="forACComponent"></param>
        public void SubscribeACObjectOnServer(IACComponent forACComponent)
        {
            if (_WCFClientManager == null || (ACOperationMode != ACOperationModes.Live))
                return;
            _WCFClientManager.SubscribeACObjectOnServer(forACComponent);
        }

        /// <summary>
        /// Method unsubscribes an unloaded ACObject
        /// </summary>
        /// <param name="forACComponent"></param>
        public void UnSubscribeACObjectOnServer(IACComponent forACComponent)
        {
            if (_WCFClientManager == null || (ACOperationMode != ACOperationModes.Live))
                return;
            _WCFClientManager.UnSubscribeACObjectOnServer(forACComponent);
        }

        /// <summary>
        /// Activates Sending of Subscription to server.
        /// Method will be called, when a common set of Objects are generated
        /// </summary>
        public void SendSubscriptionInfoToServer(bool queued)
        {
            if (_WCFClientManager == null || (ACOperationMode != ACOperationModes.Live))
                return;
            _WCFClientManager.SendSubscriptionInfoToServer(queued);
        }

        /// <summary>
        /// Makes an Entry in Dispatcher-List, that a changed Point must be send to the Server
        /// </summary>
        /// <param name="forACComponent"></param>
        public void MarkACObjectOnChangedPointForServer(IACComponent forACComponent)
        {
            if (_WCFClientManager == null || (ACOperationMode != ACOperationModes.Live))
                return;
            _WCFClientManager.MarkACObjectOnChangedPoint(forACComponent);
        }

        #endregion

        #region Server-Side-Send-Methods

        /// <summary>
        /// Sendet eine serverseitige Nachricht an alle Clients
        /// </summary>
        /// <param name="acMessage"></param>
        public void BroadcastACMessageToClients(WCFMessage acMessage)
        {
            if (_WCFServiceManager == null || (ACOperationMode != ACOperationModes.Live))
                return;
            _WCFServiceManager.BroadcastACMessageToClients(acMessage);
        }

        /// <summary>Method sends a PropertyValueEvent from this Real/Server-Object
        /// to all Proxy-Object which has subscribed ist</summary>
        /// <param name="eventArgs"></param>
        /// <param name="forACComponent"></param>
        public void BroadcastPropertyValueToClients(IACPropertyNetValueEvent eventArgs, IACComponent forACComponent)
        {
            if (_WCFServiceManager == null || (ACOperationMode != ACOperationModes.Live))
                return;
            _WCFServiceManager.BroadcastPropertyValueToClients(eventArgs, forACComponent);
        }

        /// <summary>
        /// Makes an Entry in Dispatcher-List, that a changed Point must be send to the Server
        /// </summary>
        /// <param name="forACComponent"></param>
        public void MarkACObjectOnChangedPointForClient(IACComponent forACComponent)
        {
            if (_WCFClientManager == null || (ACOperationMode != ACOperationModes.Live))
                return;
            _WCFServiceManager.MarkACObjectOnChangedPoint(forACComponent);
        }

        #endregion

        #region Verbindungsstatus
        internal void UpdateStatistic()
        {
        }

        public string ConnectionDetailXML
        {
            get
            {
                string xaml = "<ACCommunications>";
                if (_WCFClientManager != null)
                    xaml += _WCFClientManager.ConnectionDetailXML;
                if (_WCFServiceManager != null)
                    xaml += _WCFServiceManager.ConnectionDetailXML;
                xaml += "</ACCommunications>";
                return xaml;
            }
        }

        public static bool IsUserInstanceStillActive(VBUserInstance userInstance)
        {
            IPHostEntry entryToTest = null;
            List<IPAddress> addressesToTest;
            if (userInstance.NameResolutionOn)
            {
                try
                {
                    entryToTest = Dns.GetHostEntry(userInstance.Hostname);
                    if (!entryToTest.AddressList.Any())
                    {
                        return true;
                    }
                    addressesToTest = entryToTest.AddressList.Where(c => c.AddressFamily == AddressFamily.InterNetwork || c.AddressFamily == AddressFamily.InterNetworkV6).ToList();
                }
                catch (Exception e)
                {
                    string strIP = "";
                    if (userInstance.UseIPV6)
                        strIP = userInstance.ServerIPV6;
                    else
                        strIP = userInstance.ServerIPV4;

                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("Communications", "IsUserInstanceStillActive", msg);

                    if (String.IsNullOrEmpty(strIP))
                        return true;
                    IPAddress addressToTest;
                    if (!IPAddress.TryParse(strIP, out addressToTest))
                        return true;
                    addressesToTest = new List<IPAddress>();
                    addressesToTest.Add(addressToTest);
                }
            }
            else
            {
                string strIP = "";
                if (userInstance.UseIPV6)
                    strIP = userInstance.ServerIPV6;
                else
                    strIP = userInstance.ServerIPV4;
                if (String.IsNullOrEmpty(strIP))
                    return true;
                IPAddress addressToTest;
                if (!IPAddress.TryParse(strIP, out addressToTest))
                    return true;
                addressesToTest = new List<IPAddress>();
                addressesToTest.Add(addressToTest);
            }
            int port = 0;
            if (userInstance.ServiceAppEnbledHTTP)
                port = userInstance.ServicePortHTTP;
            else
                port = userInstance.ServicePortTCP;
            if (port <= 0)
                return true;

            // Check if registered IP-Address in daatabase is this localhost
            IPAddress[] ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            bool isLocal = false;
            try
            {
                foreach (IPAddress adress in addressesToTest)
                {
                    isLocal = ips.Where(c => c.ToString() == adress.ToString() 
                                        || (c.AddressFamily == AddressFamily.InterNetworkV6 && GetIPV6WithoutInterface(c.ToString()) == GetIPV6WithoutInterface(adress.ToString()))
                                       ).Any();
                    if (isLocal)
                        break;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Communications", "IsUserInstaceStillActive(10)", msg);
            }

            if (isLocal)
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] listLocalPorts = ipGlobalProperties.GetActiveTcpListeners();
                var queryPortActive = listLocalPorts.Where(c => c.Port == port);
                if (queryPortActive.Any())
                    return true;
                return false;
            }

            if (addressesToTest == null || !addressesToTest.Any())
                return false;
            bool connectionEstablished = true;
            foreach (IPAddress adress in addressesToTest)
            {
                TcpClient tcpScan = new TcpClient();
                try
                {
                    tcpScan.Connect(adress, port);
                    connectionEstablished = false;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("Communications", "IsUserInstanceStillActive(20)", msg);
                }
                tcpScan.Close();
            }
            return !connectionEstablished;
        }

        public static string GetIPV6WithoutInterface(string ipV6)
        {
            if (String.IsNullOrEmpty(ipV6))
                return ipV6;
            int ifPos = ipV6.IndexOf("%");
            if (ifPos > 0)
                ipV6 = ipV6.Substring(0, ifPos);
            return ipV6;
        }

        #endregion

    }
}
