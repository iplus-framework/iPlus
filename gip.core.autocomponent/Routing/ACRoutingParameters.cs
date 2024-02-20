using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "", Global.ACKinds.TACSimpleClass)]
    public class ACRoutingParameters
    {
        [IgnoreDataMember]
        public ACComponent RoutingService
        {
            get;
            set;
        }

        [IgnoreDataMember]
        public Database Database
        {
            get;
            set;
        }

        [DataMember]
        public bool AttachRouteItemsToContext
        {
            get;
            set;
        }

        [DataMember]
        public string SelectionRuleID
        {
            get;
            set;
        }

        [DataMember]
        public RouteDirections Direction
        {
            get;
            set;
        }

        [DataMember]
        public object[] SelectionRuleParams
        {
            get;
            set;
        }

        [IgnoreDataMember]
        public Func<ACClass, ACClassProperty, Route, bool> DBSelector
        {
            get;
            set;
        }

        [IgnoreDataMember]
        public Func<ACClass, ACClassProperty, Route, bool> DBDeSelector
        {
            get;
            set;
        }

        [DataMember]
        public int MaxRouteAlternativesInLoop
        {
            get;
            set;
        }

        [DataMember]
        public int MaxRouteLoopDepth
        {
            get;
            set;
        }

        [DataMember]
        public bool IncludeReserved
        {
            get;
            set;
        }

        [DataMember]
        public bool IncludeAllocated
        {
            get;
            set;
        }

        [DataMember]
        private bool _AutoDetachFromDBContext = false;
        public bool AutoDetachFromDBContext
        {
            get => _AutoDetachFromDBContext;
            set => _AutoDetachFromDBContext = value;
        }

        [DataMember]
        private bool _DBIncludeInternalConnections = false;
        public bool DBIncludeInternalConnections
        {
            get => _DBIncludeInternalConnections;
            set => _DBIncludeInternalConnections = value;
        }

        [DataMember]
        private int _DBRecursionLimit = 0;
        public int DBRecursionLimit
        {
            get => _DBRecursionLimit;
            set => _DBRecursionLimit = value;
        }

        [DataMember]
        private bool _DBIgnoreRecursionLoop = false;
        public bool DBIgnoreRecursionLoop
        {
            get => _DBIgnoreRecursionLoop;
            set => _DBIgnoreRecursionLoop = value;
        }

        [DataMember]
        private bool _ForceReattachToDatabaseContext = false;
        public bool ForceReattachToDatabaseContext
        {
            get => _ForceReattachToDatabaseContext;
            set => _ForceReattachToDatabaseContext = value;
        }

        [DataMember]
        private RouteResultMode _ResultMode = RouteResultMode.FullRoute;
        public RouteResultMode ResultMode
        {
            get => _ResultMode;
            set => _ResultMode = value;
        }

        [DataMember]
        public Route PreviousRoute
        {
            get;
            set;
        }

        [DataMember]
        public bool IsForEditor
        {
            get;
            set;
        }
    }
}


