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
using System.Threading;
using System.ServiceModel.Channels;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'WCFClientManager'}de{'WCFClientManager'}", Global.ACKinds.TACWCFClientManager, Global.ACStorableTypes.NotStorable, false, false)]
    public class WCFClientManager : ACComponent
    {
        #region private members

        //Thread _ACPDispatchThread = null;
        //private EventWaitHandle _waitHandleSend = null;
        #endregion

        #region c´tors

        public WCFClientManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACPostInit()
        {
            if (!base.ACPostInit())
                return false;
            OnPropertyChanged("WCFClientChannelList");
            return true;
        }
        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (!base.ACDeInit(deleteACClassTask))
                return false;

            UpdateStatistic();
            OnPropertyChanged("WCFClientChannelList");
            _CurrentWCFClientChannel = null;
            _SelectedWCFClientChannel = null;
            return true;
        }
        #endregion

        #region public members
        public override bool IsPoolable
        {
            get
            {
                return false;
            }
        }

        public Communications Communications
        {
            get
            {
                if (this.ParentACComponent is Communications)
                    return this.ParentACComponent as Communications;
                return null;
            }
        }

        [ACPropertyInfo(9999)]
        public ConnectionQuality ConnectionQuality
        {
            get
            {
                int nCountConnected = 0;
                int nCountDisConnected = 0;
                var wcfClientChannelList = WCFClientChannelList;
                if (!wcfClientChannelList.Any())
                    return ConnectionQuality.NoConnections;

                foreach (WCFClientChannel channel in wcfClientChannelList)
                {
                    if (channel.IsConnected)
                        nCountConnected++;
                    else
                        nCountDisConnected++;
                }

                if ((nCountConnected <= 0) && (nCountDisConnected > 0))
                    return ConnectionQuality.Bad;
                else if ((nCountConnected > 0) && (nCountDisConnected > 0))
                    return ConnectionQuality.Instable;
                else if ((nCountConnected > 0) && (nCountDisConnected <= 0))
                    return ConnectionQuality.Good;
                return ConnectionQuality.Good;
            }
        }

        [ACPropertyInfo(9999)]
        public string ConnectionShortInfo
        {
            get
            {
                int nCountConnected = 0;
                int nCountDisConnected = 0;

                foreach (WCFClientChannel channel in WCFClientChannelList)
                {
                    if (channel.IsConnected)
                        nCountConnected++;
                    else
                        nCountDisConnected++;
                }

                return String.Format("+{0} -{1}", nCountConnected, nCountDisConnected);
            }
        }

        internal void UpdateStatistic()
        {
            OnPropertyChanged("ConnectionQuality");
            OnPropertyChanged("ConnectionShortInfo");
            if (Communications != null)
                Communications.UpdateStatistic();
        }

        public string ConnectionDetailXML
        {
            get
            {
                var wcfClientChannelList = WCFClientChannelList;
                if (!wcfClientChannelList.Any())
                    return "";
                string xaml = "<WCFClientManager>";
                foreach (WCFClientChannel channel in wcfClientChannelList)
                {
                    xaml += channel.ConnectionDetailXML;
                }
                xaml += "</WCFClientManager>";
                return xaml;
            }
        }

        #endregion

        #region private methods

        protected WCFClientChannel GetChannelForRemoteObject(WCFMessage acMessage)
        {
            IACComponent projectService;
            return GetChannelForRemoteObject(WCFMessage.ACUrlItem(acMessage.ACUrl, 0), out projectService, true);
        }

        protected WCFClientChannel GetChannelForRemoteObject(string ACProjectIdentifier, out IACComponent projectService, bool CreateChannel)
        {
            projectService = null;
            if (ACProjectIdentifier.Length <= 0)
                return null;
            try
            {
                IACComponent acProject = Root.GetChildComponent(ACProjectIdentifier);
                if (acProject == null)
                    return null;
                projectService = acProject;
                return GetChannelForRemoteProject(projectService, CreateChannel);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("WCFClientManager", "GetChannelForRemoteObject", msg);
            }
            return null;
        }

        internal WCFClientChannel GetChannelForRemoteObject(IACComponent forACComponent, out IACComponent projectService, bool CreateChannel)
        {
            projectService = null;
            if (forACComponent == null)
                return null;
            if (forACComponent is ApplicationManagerProxy)
                projectService = forACComponent;
            else
                projectService = forACComponent.FindParentComponent<ApplicationManagerProxy>(c => c is ApplicationManagerProxy);
            // Falls Project-Root-Knoten nicht gefunden, 
            // dann ist Objekt evtl. im Debugger-Tree: \Communications\WCFClientManager\WCFClientChannel\Root\....
            if (projectService == null)
            {
                WCFClientChannel channel = forACComponent.FindParentComponent<WCFClientChannel>(c => c is WCFClientChannel);
                if (channel != null)
                    projectService = this.Root;
                return channel;
            }
            return GetChannelForRemoteProject(projectService, CreateChannel);
        }

        public WCFClientChannel GetChannelForRemoteProject(IACComponent service, bool CreateChannel)
        {
            if (service == null)
                return null;
            WCFClientChannel channelForRemoteObject = null;
            foreach (WCFClientChannel channel in WCFClientChannelList)
            {
                if (channel.IsChannelForProject(service.ACIdentifier))
                {
                    channelForRemoteObject = channel;
                    break;
                }
            }

            if (channelForRemoteObject != null)
                return channelForRemoteObject;
            if (!CreateChannel)
                return null;

            try
            {
                ACClass serviceACClass = (ACClass)service.ACType;
                lock ((serviceACClass.Database.QueryLock_1X000))
                {
                    var query = serviceACClass.ACProject.VBUserACProject_ACProject.Where(c => c.IsServer);
                    VBUserACProject userACProject = query.FirstOrDefault();
                    if (userACProject != null)
                    {
                        VBUserInstance VBUserInstance = userACProject.VBUser.VBUserInstance_VBUser.FirstOrDefault();
                        if (VBUserInstance != null)
                        {
                            channelForRemoteObject = (WCFClientChannel)StartComponent("WCFClientChannel", null, new object[] { VBUserInstance });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("WCFClientManager", "GetChannelForRemoteProject", msg);
            }

            return channelForRemoteObject;
        }

        public WCFClientChannel HasChannelForUserInstance(VBUserInstance VBUserInstance)
        {
            if (VBUserInstance == null)
                return null;
            foreach (WCFClientChannel channel in WCFClientChannelList)
            {
                if (channel.VBUserInstance == VBUserInstance)
                    return channel;
            }
            return null;
        }

        public WCFClientChannel GetChannelForUserInstance(VBUserInstance VBUserInstance)
        {
            if (VBUserInstance == null)
                return null;
            WCFClientChannel channel = HasChannelForUserInstance(VBUserInstance);
            if (channel != null)
                return channel;
            return (WCFClientChannel)StartComponent("WCFClientChannel", null, new object[] { VBUserInstance });
        }
        #endregion

        #region public methods
        /// <summary>Sendet eine Clientseitige Nachricht an den Server</summary>
        /// <param name="acMessage"></param>
        /// <param name="forACComponent"></param>
        /// <exception cref="gip.core.autocomponent.ACWCFException">Thrown when disconnected</exception>
        public void SendACMessageToServer(WCFMessage acMessage, IACComponent forACComponent)
        {
            if (ACOperationMode != ACOperationModes.Live)
                return;
            IACComponent projectService;
            WCFClientChannel clientChannel = GetChannelForRemoteObject(forACComponent, out projectService, true);
            if (clientChannel != null)
                clientChannel.SendACMessageToServer(acMessage);
        }

        /// <summary>Method sends a PropertyValueEvent from this Client/Proxy-Object
        /// to the Real Object on Server-side</summary>
        /// <param name="eventArgs"></param>
        /// <param name="forACComponent"></param>
        public void SendPropertyValueToServer(IACPropertyNetValueEvent eventArgs, IACComponent forACComponent)
        {
            if (ACOperationMode != ACOperationModes.Live)
                return;
            IACComponent projectService;
            WCFClientChannel clientChannel = GetChannelForRemoteObject(forACComponent, out projectService, true);
            if (clientChannel != null)
                clientChannel.SendPropertyValueToServer(eventArgs, forACComponent);
        }

        /// <summary>
        /// Method subscribes an new generated ACObject for retrieving ValueEvents from the Server
        /// </summary>
        /// <param name="forACComponent"></param>
        public void SubscribeACObjectOnServer(IACComponent forACComponent)
        {
            if ((forACComponent == null) || (ACOperationMode != ACOperationModes.Live))
                return;

            IACComponent projectService;
            WCFClientChannel clientChannel = GetChannelForRemoteObject(forACComponent, out projectService, true);
            if (clientChannel == null)
                return;
            if (projectService == null)
                return;

            clientChannel.SubscribeACObjectOnServer(projectService, forACComponent);
        }

        /// <summary>
        /// Method unsubscribes an unloaded ACObject
        /// </summary>
        /// <param name="forACComponent"></param>
        public void UnSubscribeACObjectOnServer(IACComponent forACComponent)
        {
            if ((forACComponent == null) || (ACOperationMode != ACOperationModes.Live))
                return;

            IACComponent projectService;
            WCFClientChannel clientChannel = GetChannelForRemoteObject(forACComponent, out projectService, true);
            if (clientChannel == null)
                return;
            if (projectService == null)
                return;

            clientChannel.UnSubscribeACObjectOnServer(projectService, forACComponent);
        }

        /// <summary>
        /// Activates Sending of Subscription to server.
        /// Method will be called, when a common set of Objects are generated
        /// </summary>
        public void SendSubscriptionInfoToServer(bool queued)
        {
            if (ACOperationMode != ACOperationModes.Live)
                return;
            foreach (WCFClientChannel channel in WCFClientChannelList)
            {
                if (queued)
                    channel.EnqueueSendSubscriptionInfoToServer();
                else
                    channel.SendSubscriptionInfoToServer();
            }
        }

        /// <summary>
        /// Makes an Entry in Dispatcher-List, that a changed Point must be send to the Server
        /// </summary>
        /// <param name="forACComponent"></param>
        public void MarkACObjectOnChangedPoint(IACComponent forACComponent)
        {
            if ((forACComponent == null) || (ACOperationMode != ACOperationModes.Live))
                return;

            IACComponent projectService;
            WCFClientChannel clientChannel = GetChannelForRemoteObject(forACComponent, out projectService, true);
            if (clientChannel == null)
                return;
            if (projectService == null)
                return;

            clientChannel.MarkACObjectOnChangedPoint(projectService, forACComponent);
        }

        public void BroadcastShutdownAllClients()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return;
            foreach (WCFClientChannel channel in WCFClientChannelList)
            {
                channel.BroadcastShutdownAllClients();
            }
        }

        #endregion

        #region DataShow
        WCFClientChannel _CurrentWCFClientChannel;
        [ACPropertyCurrent(9999, "WCFClientChannel")]
        public WCFClientChannel CurrentWCFClientChannel
        {
            get
            {
                return _CurrentWCFClientChannel;
            }
            set
            {
                _CurrentWCFClientChannel = value;
                OnPropertyChanged("CurrentWCFClientChannel");
            }
        }

        [ACPropertyList(9999, "WCFClientChannel")]
        public IEnumerable<WCFClientChannel> WCFClientChannelList
        {
            get
            {
                return ACComponentChilds.Where(c => c is WCFClientChannel).Select(c => c as WCFClientChannel);
            }
        }


        WCFClientChannel _SelectedWCFClientChannel;
        [ACPropertySelected(9999, "WCFClientChannel")]
        public WCFClientChannel SelectedWCFClientChannel
        {
            get
            {
                return _SelectedWCFClientChannel;
            }
            set
            {
                _SelectedWCFClientChannel = value;
                OnPropertyChanged("SelectedWCFClientChannel");
            }
        }
        #endregion
    }

}
