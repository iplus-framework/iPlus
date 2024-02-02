using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [DataContract]
    public class ACRoutingVertex
    {
        #region c'tors

        public ACRoutingVertex(IACComponent component)
        {
            Component = new ACRef<IACComponent>(component, ACRef<IACComponent>.RefInitMode.AutoStartStop, false, null);
            RelatedEdges = Component.ValueT.ACPointList.OfType<PAPoint>()
                                                       //.Select(x => ((PAPoint)x).ConnectionList
                                                       .Select(x => ((PAPoint)x).ConnectionList.Where(c => (c as PAEdge).Relation.ConnectionType == Global.ConnectionTypes.ConnectionPhysical)
                                                       .Where(k => k.Target.ParentACComponent == Component.ValueT || k.Source.ParentACComponent == Component.ValueT))
                                                       .SelectMany(x => x).ToList();


            //RelatedEdges = Component.ValueT.ACPointList.Where(c => c.GetType() == typeof(PAPoint))
            //                                   .Select(x => ((PAPoint)x).ConnectionList
            //                                   .Where(k => k.Target.ParentACComponent == Component.ValueT || k.Source.ParentACComponent == Component.ValueT))
            //                                   .SelectMany(x => x).ToList();
            ResetRoutingState();
        }

        public ACRoutingVertex(IACComponent component, Guid targetPointACClassPropertyID)
        {
            Component = new ACRef<IACComponent>(component, ACRef<IACComponent>.RefInitMode.AutoStartStop, false, null);

            RelatedEdges = Component.ValueT.ACPointList.OfType<PAPoint>().FirstOrDefault(c => c.PropertyInfo.ACClassPropertyID == targetPointACClassPropertyID)
                                                       .ConnectionList.Where(x => x.Relation.ConnectionType == Global.ConnectionTypes.ConnectionPhysical).ToList();

            ResetRoutingState();
        }

        #endregion

        #region Properties

        [IgnoreDataMember]
        private ACRef<IACComponent> _Component;
        [DataMember]
        public ACRef<IACComponent> Component
        {
            get
            {
                return _Component;
            }
            set
            {
                _Component = value;
            }
        }

        [IgnoreDataMember]
        private List<PAEdge> _RelatedEdges;
        [DataMember]
        public List<PAEdge> RelatedEdges
        {
            get
            {
                return _RelatedEdges;
            }
            set
            {
                _RelatedEdges = value;
            }
        }

        public IEnumerable<PAEdge> FromEdges
        {
            get
            {
                return RelatedEdges?.Where(c => c.SourceParentComponent == Component?.ValueT);
            }
        }

        public IEnumerable<PAEdge> ToEdges
        {
            get
            {
                return RelatedEdges?.Where(c => c.TargetParentComponent == Component?.ValueT);
            }
        }

        [DataMember]
        public int Distance
        {
            get;
            set;
        }

        [DataMember]
        public int? DistanceInLoop
        {
            get;
            set;
        }

        [DataMember]
        public PAEdge EdgeToPath
        {
            get;
            set;
        }
        
        [IgnoreDataMember]
        public ACRef<IACComponent> Next
        {
            get
            {
                //if (EdgeToPath == null)
                //    return null;

                //if (_Next == null || _Next.ValueT != EdgeToPath.Target.ParentACComponent)
                //    _Next = new ACRef<ACComponent>(EdgeToPath.Target.ParentACComponent as ACComponent, true);

                //return _Next;

                return EdgeToPath == null ? null : EdgeToPath.Target.ACRef;
            }
        }

        [DataMember]
        public bool HasAnyReserved
        {
            get;
            set;
        }

        [DataMember]
        public bool HasAnyAllocated
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public void ResetRoutingState()
        {
            Distance = int.MinValue;
            EdgeToPath = null;
            DistanceInLoop = null;
            HasAnyReserved = false;
            HasAnyAllocated = false;
        }

        public override string ToString()
        {
            return Component != null ? Component.ValueT.ACUrl + "  :  " + Distance : "";
        }

        #endregion  
    }
}
