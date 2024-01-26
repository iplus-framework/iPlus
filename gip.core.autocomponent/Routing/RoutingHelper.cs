using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [CollectionDataContract]
    public class ACRoutingPath : List<PAEdge>
    {
        #region Properties

        [DataMember]
        public bool IsPrimaryRoute
        {
            get;
            set;
        }

        [IgnoreDataMember]
        public bool IsValid
        {
            get
            {
                return this.Any();
            }
        }

        /// <summary>
        /// Returns sum of all deltas of edges in the RoutingPath
        /// </summary>
        [IgnoreDataMember]
        public int DeltaWeight
        {
            get
            {
                int total = 0;
                foreach (PAEdge e in this)
                    total += e.Delta;
                return total;
            }
        }

        [IgnoreDataMember]
        public int RouteWeight
        {
            get
            {
                int total = 0;
                foreach (PAEdge e in this)
                    total += e.Weight;
                return total;
            }
        }

        [IgnoreDataMember]
        public List<IACPointBase> AllPoints
        {
            get
            {
                var points = this.Select(c => c.Source).ToList();
                points.Add(this.LastOrDefault().Target);
                return points;
            }
        }

        [IgnoreDataMember]
        public IACPointBase Start
        {
            get
            {
                return this.FirstOrDefault().Source;
            }
        }

        [IgnoreDataMember]
        public IACPointBase End
        {
            get
            {
                return this.LastOrDefault().Target;
            }
        }

        #endregion
    }

    /// <summary>
    /// Class for nodes in shortest RoutingPath tree, with comparing capability
    /// </summary>
    public class SP_Node : IComparable
    {
        /// <summary>
        /// Contained edge
        /// </summary>
        public PAEdge Edge;
        /// <summary>
        /// Weight of node (edge)
        /// </summary>
        public int Weight;

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="_edge">Edge object</param>
        /// <param name="_weight">Weight of node</param>
        public SP_Node(PAEdge _edge, int _weight)
        {
            this.Edge = _edge;
            this.Weight = _weight;
        }

        /// <summary>
        /// Implements IComparable's CompareTo method
        /// Compare two SP_Node objects by it weight values
        /// </summary>
        /// <param name="_obj">Object to compare with</param>
        /// <returns>-1 if this is shorter than object, 1 if countersense, 0 if both are equal</returns>
        /// <exception cref="System.Exception">Thrown when obj is not SP_Node type</exception>
        int IComparable.CompareTo(object _obj)
        {
            return Math.Sign(this.Weight - ((SP_Node)_obj).Weight);
        }
        /// <summary>
        /// Returns node's descriptive string
        /// </summary>
        /// <returns>Edge and weight string</returns>
        public override string ToString()
        {
            return /*this.Edge+*/" [" + Weight + "]";
        }
    }

    /// <summary>
    /// Sidetracks collection, with comparing capability
    /// </summary>
    [DataContract]
    public class ST_Node : IComparable
    {
        /// <summary>
        /// Collection of edges (sidetracks), not exactly a full RoutingPath
        /// </summary>
        [DataMember]
        public ACRoutingPath Sidetracks
        {
            get;
            set;
        }
        /// <summary>
        /// Weight of node (sum of sidetracks)
        /// </summary>
        [DataMember]
        public int Weight
        {
            get;
            set;
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="_sidetracks">Sidetrack collection</param>
        /// <remarks>Weight is calculated using RoutingPath's weight</remarks>
        public ST_Node(ACRoutingPath _sidetracks)
        {
            this.Sidetracks = _sidetracks;
            this.Weight = _sidetracks.DeltaWeight;
        }

        /// <summary>
        /// Implements IComparable's CompareTo method
        /// Compare two ST_Node objects by it weight values
        /// </summary>
        /// <param name="_obj">Object to compare with</param>
        /// <returns>-1 if this is shorter than object, 1 if countersense, 0 if both are equal</returns>
        /// <exception cref="System.Exception">Thrown when obj is not ST_Node type</exception>
        int IComparable.CompareTo(object _obj)
        {
            return Math.Sign(this.Weight - ((ST_Node)_obj).Weight);
        }
        /// <summary>
        /// Returns node's descriptive string
        /// </summary>
        /// <returns>Weight as string</returns>
        public override string ToString()
        {
            return /*this.Sidetracks+*/" [" + Weight + "]";
        }
    }

    public class PAEdgeInfo
    {
        public PAEdgeInfo(PAEdge edge, short useFactor)
        {
            this.Edge = edge;
            this.LastManipulationTime = edge.Relation != null ? edge.Relation.LastManipulationDT : DateTime.Now;
            this.UseFactor = useFactor;
        }

        public PAEdge Edge
        {
            get;
            set;
        }

        public DateTime LastManipulationTime
        {
            get;
            set;
        }

        public short UseFactor
        {
            get;
            set;
        }

        private int IncreaseUseFactor
        {
            get
            {
                switch(UseFactor)
                {
                    case 0:
                        return 1;
                    case 1:
                        return 3;
                    case 2:
                        return 3;
                    case 3:
                        return 2;
                    case 4:
                        return 2;
                    case 5:
                        return 1;
                    default:
                        return 1;
                }
            }
        }

        public PAEdgeInfo DecreaseEdgeWeight()
        {
            if(UseFactor < 5)
                UseFactor++;
            int newWeight = Edge.Weight - UseFactor;
            if (newWeight < 1)
                newWeight = 1;
            Edge.Weight = newWeight;
            LastManipulationTime = DateTime.Now;

            return this;
        }

        public PAEdgeInfo IncreaseEdgeWeight()
        {
            if (UseFactor > 1)
                UseFactor--;

            int newWeight = Edge.Weight + IncreaseUseFactor;
            if (newWeight > 100)
                newWeight = 100;

            Edge.Weight = newWeight;
            LastManipulationTime = DateTime.Now;

            return this;
        }
    }

    public class SelectionRule
    {
        public Func<ACRoutingVertex, object[], bool> Selector { get; set; }
        public Func<ACRoutingVertex, object[], bool> DeSelector { get; set; }
    }

    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RouteDirections'}de{'RouteDirections'}", Global.ACKinds.TACEnum)]
    public enum RouteDirections : int
    {
        Backwards,
        Forwards
    }

    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RoutingResult'}de{'RoutingResult'}", Global.ACKinds.TACSimpleClass)]
    public class RoutingResult
    {
        public RoutingResult(IEnumerable<Route> routes, bool isDbResult, Msg msg, IEnumerable<ACRef<IACComponent>> components = null)
        {
            _Routes = routes;
            _IsDbResult = isDbResult;
            _Msg = msg;
            _Components = components;
        }

        [IgnoreDataMember]
        private IEnumerable<Route> _Routes;
        [DataMember]
        public IEnumerable<Route> Routes
        {
            get
            {
                return _Routes;
            }
            set
            {
                _Routes = value;
            }
        }

        [IgnoreDataMember]
        private IEnumerable<ACRef<IACComponent>> _Components;
        [DataMember]
        public IEnumerable<ACRef<IACComponent>> Components
        {
            get
            {
                return _Components;
            }
            set
            {
                _Components = value;
            }
        }

        [IgnoreDataMember]
        private bool _IsDbResult;
        [DataMember]
        public bool IsDbResult
        {
            get
            {
                return _IsDbResult;
            }
            set
            {
                _IsDbResult = value;
            }
        }

        [IgnoreDataMember]
        private Msg _Msg;
        [DataMember]
        public Msg Message
        {
            get
            {
                return _Msg;
            }
            set
            {
                _Msg = value;
            }
        }
    }

    [ACSerializeableInfo]
    [CollectionDataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RoutingResult'}de{'RoutingResult'}", Global.ACKinds.TACSimpleClass)]
    public class GuidList : List<Guid>
    {
        public GuidList()
        {
                
        }

        public GuidList(IEnumerable<Guid> items) : base(items)
        {
            
        }
    }

    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RouteResultMode'}de{'RouteResultMode'}", Global.ACKinds.TACEnum)]
    public enum RouteResultMode : short
    {
        ShortRoute = 0,
        FullRouteFromFindComp = 10,
        FullRoute = 20
    }
}
