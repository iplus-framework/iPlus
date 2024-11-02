// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class ACPDispatchService : ACPDispatchBase
    {
        #region c'tors
        public ACPDispatchService()
            : base()
        {
            _SubscribedOrChangedPoints = new ACPSubscrDispService();
        }
        #endregion

        #region Members
        public override bool IsServer
        {
            get
            {
                return true;
            }
        }

        internal bool EmptyAllValueEvents
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private ACPSubscrDispService _SubscribedOrChangedPoints;

        public ACPSubscrDispService SubscribedOrChangedPoints
        {
            get
            {
                return _SubscribedOrChangedPoints;
            }
        }

        #endregion

        #region Methods

        private bool _HasBroadcastedEvents = false;

        public List<IACPropertyNetValueEvent> GetValueEventsForSubscription(ACPSubscrBase forSubscription)
        {
            List<IACPropertyNetValueEvent> eventList = new List<IACPropertyNetValueEvent>();
            if (forSubscription == null)
                return eventList;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {

                using (ACMonitor.Lock(forSubscription._20054_LockProjectSubscriptionList))
                {
                    String prevACUrl = ""; // Telegrammoptimization for smaller size
                    foreach (ACPDispatchProj DispatchProject in _ProjectDispatchList)
                    {
                        ACPSubscrProjBase subscriptionProject = forSubscription.GetProject(DispatchProject.ACIdentifier, false);
                        if (subscriptionProject != null)
                        {
                            foreach (IACPropertyNetValueEvent valueEvent in DispatchProject.ValueEventDispatchList)
                            {
                                valueEvent.EventBroadcasted = ACPropertyBroadcastState.ProcessedInLoop;
                                ACPSubscrObjBase subscribedObject = subscriptionProject.GetACObject(valueEvent.ACUrl);
                                if (subscribedObject != null)
                                {
                                    // Optimisation for shorter WCF-Message length
                                    IACPropertyNetValueEvent valueEventClone = (valueEvent as IACPropertyNetValueEventExt).Clone();
                                    if (prevACUrl == valueEventClone.ACUrl)
                                        (valueEventClone as IACPropertyNetValueEventExt).SetACUrl();
                                    else
                                        prevACUrl = valueEventClone.ACUrl;

                                    eventList.Add(valueEventClone);
                                    valueEvent.SubscriptionSendCount++;
                                    valueEventClone.SubscriptionSendCount++;
                                    if (subscribedObject.SharedSubscriptionObject != null)
                                    {
                                        if (subscribedObject.SharedSubscriptionObject.RefCounter <= valueEvent.SubscriptionSendCount)
                                        {
                                            valueEvent.EventBroadcasted = ACPropertyBroadcastState.PreparedToSend;
                                            valueEventClone.EventBroadcasted = ACPropertyBroadcastState.PreparedToSend;
                                        }
                                    }
                                    _HasBroadcastedEvents = true;
                                }
                            }
                        }
                    }
                }
            }
            return eventList;
        }

        private class MapChannelWithSubscr
        {
            public WCFServiceChannel Channel { get; set; }
            public ACPSubscrProjBase SubscrProj { get; set; }
            public String prevACUrl { get; set; } // Telegrammoptimization for smaller size
        }

        public void GetValueEventsForSubscription(IEnumerable<WCFServiceChannel> channelList)
        {
            List<WCFServiceChannel> channelListToSend = new List<WCFServiceChannel>();

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                try
                {
                    foreach (WCFServiceChannel channel in channelList)
                    {
                        if (channel.ServiceOfPeer != null && !channel.ServiceOfPeer.ClosingConnection && channel.SubscriptionOfPeer != null)
                        {
                            if (channel.SubscriptionOfPeer.TryEnterCS())
                            {
                                channelListToSend.Add(channel);
                            }
                        }
                    }

                    foreach (ACPDispatchProj DispatchProject in _ProjectDispatchList)
                    {
                        List<MapChannelWithSubscr> mapChannelSubscrList = new List<MapChannelWithSubscr>();
                        foreach (WCFServiceChannel channel in channelListToSend)
                        {
                            ACPSubscrProjBase subscriptionProject = channel.SubscriptionOfPeer.GetProject(DispatchProject.ACIdentifier, false);
                            if (subscriptionProject != null)
                            {
                                mapChannelSubscrList.Add(new MapChannelWithSubscr() { Channel = channel, SubscrProj = subscriptionProject, prevACUrl = "" });
                            }
                        }
                        if (mapChannelSubscrList.Any())
                        {
                            foreach (IACPropertyNetValueEvent valueEvent in DispatchProject.ValueEventDispatchList)
                            {
                                valueEvent.EventBroadcasted = ACPropertyBroadcastState.ProcessedInLoop;
                                foreach (var mapChannelObj in mapChannelSubscrList)
                                {
                                    ACPSubscrObjBase subscribedObject = mapChannelObj.SubscrProj.GetACObject(valueEvent.ACUrl);
                                    if (subscribedObject != null)
                                    {
                                        // Optimisation for shorter WCF-Message length
                                        IACPropertyNetValueEvent valueEventClone = (valueEvent as IACPropertyNetValueEventExt).Clone();
                                        if (mapChannelObj.prevACUrl == valueEventClone.ACUrl)
                                            (valueEventClone as IACPropertyNetValueEventExt).SetACUrl();
                                        else
                                            mapChannelObj.prevACUrl = valueEventClone.ACUrl;

                                        if (mapChannelObj.Channel._PropValuesToSend == null)
                                        {
                                            mapChannelObj.Channel._PropValuesToSend = new ACPropertyValueMessage();
                                            mapChannelObj.Channel._PropValuesToSend.PropertyValues = new List<IACPropertyNetValueEvent>();
                                        }
                                        mapChannelObj.Channel._PropValuesToSend.PropertyValues.Add(valueEventClone);
                                        valueEvent.SubscriptionSendCount++;
                                        valueEventClone.SubscriptionSendCount++;
                                        if (subscribedObject.SharedSubscriptionObject != null)
                                        {
                                            if (subscribedObject.SharedSubscriptionObject.RefCounter <= valueEvent.SubscriptionSendCount)
                                            {
                                                valueEvent.EventBroadcasted = ACPropertyBroadcastState.Sent;
                                                valueEventClone.EventBroadcasted = ACPropertyBroadcastState.Sent;
                                            }
                                        }
                                        _HasBroadcastedEvents = true;
                                    }
                                }
                                // Fehler: TODO Log
                                if (valueEvent.EventBroadcasted != ACPropertyBroadcastState.Sent)
                                {
                                    valueEvent.EventBroadcasted = ACPropertyBroadcastState.Sent;
                                }
                            }
                        }
                    }
                }
                // TODO Errorlog
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPDispatchService", "GetValueEventsForSubscription", msg);
                }
                finally
                {
                    foreach (WCFServiceChannel channel in channelListToSend)
                    {
                        if (channel.SubscriptionOfPeer != null)
                        {
                            channel.SubscriptionOfPeer.LeaveCS();
                        }
                    }
                }
            }
        }


        public void MarkEventsAsSended(IEnumerable<IACPropertyNetValueEvent> eventsToMarkSended)
        {
            if (eventsToMarkSended == null)
                return;
            foreach (IACPropertyNetValueEvent acpEvent in eventsToMarkSended)
            {
                acpEvent.EventBroadcasted = ACPropertyBroadcastState.Sent;
                if ((acpEvent as IACPropertyNetValueEventExt).OriginalEventIfClone != null)
                    (acpEvent as IACPropertyNetValueEventExt).OriginalEventIfClone.EventBroadcasted = ACPropertyBroadcastState.Sent;
            }
        }


        public void RemoveAllSentEvents(bool withAging = false)
        {
            if (_HasBroadcastedEvents == false)
                return;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                _HasBroadcastedEvents = false;
                foreach (ACPDispatchProj DispatchProject in _ProjectDispatchList)
                {
                    try
                    {
                        //if (EmptyAllValueEvents)
                        //{
                        //    DispatchProject.RemoveAllTriedToSend();
                        //}
                        //else
                        //{
                        DispatchProject.RemoveAllSent(withAging);
                        //}
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACPDispatchService", "RemoveAllSentEvents", msg);
                    }
                }
            }
        }

        public void RemoveAllTriedToSendEvents()
        {
            if (_HasBroadcastedEvents == false)
                return;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                _HasBroadcastedEvents = false;
                foreach (ACPDispatchProj DispatchProject in _ProjectDispatchList)
                {
                    try
                    {
                        DispatchProject.RemoveAllTriedToSend();
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACPDispatchService", "RemoveAllTriedToSendEvents", msg);
                    }
                }
            }
        }


        public bool MarkACObjectOnChangedPoint(IACComponent acComponentProject, IACComponent ChildNode)
        {
            if ((acComponentProject == null) || (ChildNode == null))
                return false;
            bool result = false;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                result = _SubscribedOrChangedPoints.Subscribe(acComponentProject, ChildNode);
            }
            return result;
        }

        public ACPSubscrDispService GetChangedPointsForSubscription(ACPSubscrService forSubscription)
        {
            ACPSubscrDispService send_SubscrDispService = new ACPSubscrDispService();
            if (forSubscription == null)
                return send_SubscrDispService;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {

                using (ACMonitor.Lock(forSubscription._20054_LockProjectSubscriptionList))
                {
                    // Durchlaufe Liste von gemerkten Projekten ("changed_"), deren Points geändert wurden
                    foreach (ACPSubscrProjDispService changed_SubscrDispProj in _SubscribedOrChangedPoints.ProjectSubscriptionList)
                    {
                        ACPSubscrProjDispService send_SubscrDispProj = null;

                        // Hat der Netzwerk-Client dieses Projekt abonniert?
                        ACPSubscrProjService subscribedProject = (ACPSubscrProjService)forSubscription.GetProject(changed_SubscrDispProj.ACIdentifier, false);
                        if (subscribedProject != null)
                        {
                            // Durchlaufe Liste von gemerkten Objekten ("changed_"), deren Points geändert wurden
                            foreach (ACPSubscrObjDispService changed_SubscrDispObj in changed_SubscrDispProj.SubscribedACObjectsDict.Values)
                            {
                                // Hat der Netzwerk-Client dieses Objekt abonniert?
                                ACPSubscrObjService subscribedObject = (ACPSubscrObjService)subscribedProject.GetACObject(changed_SubscrDispObj.GetACUrl());
                                if (subscribedObject != null)
                                {
                                    // Is dieses "Netzwerk-Client-Objekt" eines, das geänderte Points dafür hat?
                                    if (!subscribedObject.HasChangedPoints)
                                        continue;

                                    // Fertige eine Kopie der geänderten Point Liste an und baue Baumstruktur für den Versand auf
                                    ACPSubscrObjDispService send_SubscrDispObj = new ACPSubscrObjDispService(changed_SubscrDispObj.GetACUrl());

                                    // Kopiere geänderte Quell-Point-Liste und lösche automatisch den Merker "PointChangedForBroadcast" in der Quelliste
                                    send_SubscrDispObj.ChangedPointList = subscribedObject.ChangedPointList;
                                    if (send_SubscrDispProj == null)
                                    {
                                        send_SubscrDispProj = new ACPSubscrProjDispService(changed_SubscrDispProj.ACIdentifier);
                                        send_SubscrDispService.ProjectSubscriptionList.Add(send_SubscrDispProj);
                                    }
                                    send_SubscrDispProj.SubscribedACObjectsDict.Add(send_SubscrDispObj.ACUrl, send_SubscrDispObj);
                                }
                            }
                        }
                    }
                }
            }
            return send_SubscrDispService;
        }

        internal void RemoveAllMarkedObjects()
        {

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                _SubscribedOrChangedPoints = new ACPSubscrDispService();
            }
        }

#endregion

    }

}
