using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    public class ACPSubscrObjService : ACPSubscrObjBase
    {
        #region c'tors
        public ACPSubscrObjService()
            : base()
        {
        }

        public ACPSubscrObjService(string ACUrl, ACPSubscrProjService parent)
            : base(ACUrl)
        {
            _Parent = parent;
        }

        #endregion

        #region Members
        public void UpdateConnectionPoints(ACPSubscrObjDispClient update, List<Tuple<IACPointNetBase, IACPointNetBase>> broadcastList)
        {
            if (update.UpdatePointList != null)
            {
                foreach (IACPointNetBase updatePoint in update.UpdatePointList)
                {
                    updatePoint.RebuildAfterDeserialization(this);

                    IACPointNetBase existingPoint = ClientsPointList.Where(c => c.ACIdentifier == updatePoint.ACIdentifier).FirstOrDefault();
                    // Füge neuen Punkt in Liste hinzu, falls er noch nicht existierte
                    if (existingPoint == null)
                    {
                        ClientsPointList.Add(updatePoint);
                        existingPoint = updatePoint;
                    }
                    broadcastList.Add(new Tuple<IACPointNetBase, IACPointNetBase>(existingPoint, updatePoint));
                    // Change due to Deadlock-Avoidance
                    //existingPoint.OnPointReceivedRemote(updatePoint);
                }

                // Rücksenden, falls aktualisiert und notwendig
            }
        }

        public void UnSubscribe()
        {
            Detach();
            foreach (IACPointNetBase updatePoint in ClientsPointList)
            {
                updatePoint.UnSubscribe();
            }
            ACPSubscrService.UnSubscribeACObjectOverAllProxies(this);
        }

        [IgnoreDataMember]
        protected List<IACPointNetBase> _ClientsPointList = null;

        [IgnoreDataMember]
        public List<IACPointNetBase> ClientsPointList
        {
            get
            {
                if (_ClientsPointList == null)
                    _ClientsPointList = new List<IACPointNetBase>();
                return _ClientsPointList;
            }
        }

        public IACPointNetBase GetConnectionPoint(string acConnectionPointName)
        {
            // Propertyname per Reflection?
            // Abfrage nach To/From, Nach Event, Request, Object//Nach WrapperType

            if ((_ClientsPointList == null) || (acConnectionPointName.Length <= 0))
                return null;
            try
            {
                return ClientsPointList.Where(c => c.ACIdentifier == acConnectionPointName).FirstOrDefault();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrObjService", "GetConnectionPoint", msg);
            }
            return null;
        }
 

        [IgnoreDataMember]
        public bool HasChangedPoints
        {
            get
            {
                if (!ClientsPointList.Any())
                    return false;
                return ClientsPointList.Where(c => c.PointChangedForBroadcast).Any();
            }
        }


        [IgnoreDataMember]
        internal List<IACPointNetBase> ChangedPointList
        {
            // Abfrage von ACPDispatchService:
            get
            {
                List<IACPointNetBase> _ChangedPointList = new List<IACPointNetBase>();
                if (ClientsPointList.Any())
                {
                    _ChangedPointList = ClientsPointList.Where(c => c.PointChangedForBroadcast).ToList();
                    if (_ChangedPointList.Any())
                        _ChangedPointList.ForEach(c => c.PointChangedForBroadcast = false);
                }
                return _ChangedPointList;
            }
        }

        ACPSubscrProjService _Parent = null;
        public ACPSubscrProjService Parent
        {
            get
            {
                return _Parent;
            }
        }

        public WCFServiceChannel WCFServiceChannel
        {
            get
            {
                if (Parent == null)
                    return null;
                return Parent.WCFServiceChannel;
            }
        }

        #endregion
    }

    public class ACPSubscrObjServiceShared : ACPSubscrObjBase
    {
        #region c'tors
        public ACPSubscrObjServiceShared(string ACUrl)
            : base(ACUrl)
        {
            RefCounter = 0;
        }
        #endregion

        #region Members
        internal int RefCounter { get; set; }
        #endregion
    }
}
