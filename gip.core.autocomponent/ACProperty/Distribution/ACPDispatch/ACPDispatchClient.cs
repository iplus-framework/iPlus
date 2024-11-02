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
    public class ACPDispatchClient : ACPDispatchBase
    {
        #region c'tors
        public ACPDispatchClient()
            : base()
        {
            _SubscribeRequest = new ACPSubscrDispClient();
            _UnSubscribeRequest = new ACPSubscrDispClient();
            //_SubscribedOrChangedPoints = new ACPSubscrDispClient();
        }
        #endregion

        #region Members
        /// <summary>
        /// Liste mit temporären Einträgen, die beim Laden/Nachladen der ACObjekte gesammelt gefüllt wird.
        /// Danach wird das Abo an den Server übertragen und die Liste geleert.
        /// </summary>
        private ACPSubscrDispClient _SubscribeRequest;

        public ACPSubscrDispClient SubscribeRequest
        {
            get
            {
                return _SubscribeRequest;
            }
        }

        private bool _InACObjectInitPhase = false;
        /// <summary>
        /// Kennzeichen, dass neue Subscription-Objekte angelegt worden sind aufgrund von
        /// der Instantierung von neuen ACObjects. Folglich wird SendSubscriptionInfoToServer
        /// aufgerufen, und bislang eingetragen Subscription-Objekte durch MarkACObjectOnChangedPoint
        /// müssen nicht über DispatchPoints() versendet werden.
        /// </summary>
        public bool InACObjectInitPhase
        {
            get
            {
                return _InACObjectInitPhase;
            }
        }

        /// <summary>
        /// Liste mit temporären Einträgen, die beim Entladen der ACObjekte gesammelt gefüllt wird.
        /// Liste wird auch gefüllt, wenn vom Server ACPropertyValueEvents für ACObjekte gesendet werden, 
        /// die entladen sind.
        /// Danach wird das Abmelde-Abo an den Server übertragen und die Liste geleert.
        /// </summary>
        private ACPSubscrDispClient _UnSubscribeRequest;

        public ACPSubscrDispClient UnSubscribeRequest
        {
            get
            {
                return _UnSubscribeRequest;
            }
        }

        /*private ACPSubscrDispClient _SubscribedOrChangedPoints;

        public ACPSubscrDispClient SubscribedOrChangedPoints
        {
            get
            {
                return _SubscribedOrChangedPoints;
            }
        }*/

        #endregion

        #region Methods
        public bool Subscribe(IACComponent acComponentProject, IACComponent ChildNode)
        {
            return Subscribe(acComponentProject, ChildNode, false);
        }

        private bool Subscribe(IACComponent acComponentProject, IACComponent ChildNode, bool IsPointChange)
        {
            if ((acComponentProject == null) || (ChildNode == null))
                return false;
            bool result = false;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                result = _SubscribeRequest.Subscribe(acComponentProject, ChildNode);
            }
            return result;
        }

        public bool UnSubscribe(IACComponent acComponentProject, IACComponent ChildNode)
        {
            if ((acComponentProject == null) || (ChildNode == null))
                return false;
            bool result = false;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                result = _UnSubscribeRequest.Subscribe(acComponentProject, ChildNode);
            }
            return result;
        }


        public void CountOfSubscriptions(out int countSubscr, out int countUnscubscr)
        {
            countSubscr = 0;
            countUnscubscr = 0;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                countSubscr = SubscribeRequest.ProjectSubscriptionList.Count;
                countUnscubscr = UnSubscribeRequest.ProjectSubscriptionList.Count;
            }
        }

        public bool GetCopyAndEmpty(ref ACPSubscrDispClient ACPSubscribe, ref ACPSubscrDispClient ACPUnSubscribe)
        {
            bool succ = false;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                ACPSubscribe = _SubscribeRequest;
                ACPUnSubscribe = _UnSubscribeRequest;
                _SubscribeRequest = new ACPSubscrDispClient();
                _UnSubscribeRequest = new ACPSubscrDispClient();
                succ = true;
            }
            return succ;
        }

        public void EmptySubscriptionRequests()
        {

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                _SubscribeRequest.UnSubscribeAll();
                _UnSubscribeRequest.UnSubscribeAll();
            }
        }

        public bool MarkACObjectOnChangedPoint(IACComponent acComponentProject, IACComponent ChildNode)
        {
            return Subscribe(acComponentProject, ChildNode, true);
        }

        public List<IACPropertyNetValueEvent> GetAllEvents(bool AutoRemove)
        {
            List<IACPropertyNetValueEvent> eventList = new List<IACPropertyNetValueEvent>();

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                foreach (ACPDispatchProj DispatchProject in _ProjectDispatchList)
                {
                    eventList.AddRange(DispatchProject.ValueEventDispatchList);
                }
                if (AutoRemove)
                    _ProjectDispatchList = new List<ACPDispatchProj>();
            }
            return eventList;
        }

#endregion

    }

}
