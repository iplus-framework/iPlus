using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public ACRoutingVertex(IACComponent component, bool includeACRef)
        {
            _ComponentInstance = component;
            if (includeACRef)
            {
                _ComponentRef = new ACRef<IACComponent>(component, ACRef<IACComponent>.RefInitMode.AutoStartStop, false, null);
            }


            IACComponent compInstance = _ComponentInstance;

            _RelatedEdges = _ComponentInstance.ACPointList.OfType<PAPoint>()
                                                       //.Select(x => ((PAPoint)x).ConnectionList
                                                       .Select(x => ((PAPoint)x).ConnectionList.Where(c => (c as PAEdge).Relation.ConnectionType == Global.ConnectionTypes.ConnectionPhysical)
                                                       .Where(k => k.Target.ParentACComponent == compInstance || k.Source.ParentACComponent == compInstance))
                                                       .SelectMany(x => x).ToArray();
            ResetRoutingState();
        }

        public ACRoutingVertex(IACComponent component, Guid targetPointACClassPropertyID, bool includeACRef)
        {
            _ComponentInstance = component;

            if (includeACRef)
            {
                _ComponentRef = new ACRef<IACComponent>(component, ACRef<IACComponent>.RefInitMode.AutoStartStop, false, null);
            }

            _RelatedEdges = _ComponentInstance.ACPointList.OfType<PAPoint>().FirstOrDefault(c => c.PropertyInfo.ACClassPropertyID == targetPointACClassPropertyID)
                                                       .ConnectionList.Where(x => x.Relation.ConnectionType == Global.ConnectionTypes.ConnectionPhysical).ToArray();
            ResetRoutingState();
        }

        #endregion

        #region Properties

        [IgnoreDataMember]
        private ACRef<IACComponent> _ComponentRef;
        [DataMember]
        public ACRef<IACComponent> ComponentRef
        {
            get
            {
                return _ComponentRef;
            }
            set
            {
                _ComponentRef = value;
            }
        }

        [IgnoreDataMember]
        private IACComponent _ComponentInstance;
        [IgnoreDataMember]
        public IACComponent ComponentInstance
        {
            get
            {
                return _ComponentInstance;
            }
            set
            {
                _ComponentInstance = value;
            }
        }

        public IACComponent Component
        {
            get
            {
                if (ComponentInstance != null)
                    return ComponentInstance;

                if (ComponentRef != null)
                    return ComponentRef.ValueT;

                return null;
            }
        }

        [IgnoreDataMember]
        private PAEdge[] _RelatedEdges;
        [DataMember]
        public PAEdge[] RelatedEdges
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
                var compInstance = ComponentInstance;
                return RelatedEdges?.Where(c => c.SourceParentComponent == compInstance);
            }
        }

        public IEnumerable<PAEdge> ToEdges
        {
            get
            {
                var compInstance = ComponentInstance;
                return RelatedEdges?.Where(c => c.TargetParentComponent == compInstance);
            }
        }

        [IgnoreDataMember]
        private int _Distance;
        [DataMember]
        public int Distance
        {
            get => _Distance;
            set => _Distance = value;
        }

        [IgnoreDataMember]
        public int? _DistanceInLoop;
        [DataMember]
        public int? DistanceInLoop
        {
            get => _DistanceInLoop;
            set => _DistanceInLoop = value;
        }

        [IgnoreDataMember]
        private PAEdge _EdgeToPath;
        [DataMember]
        public PAEdge EdgeToPath
        {
            get => _EdgeToPath;
            set => _EdgeToPath = value;
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

        [IgnoreDataMember]
        private bool _HasAnyReserved;
        [DataMember]
        public bool HasAnyReserved
        {
            get => _HasAnyReserved;
            set => _HasAnyReserved = value;
        }

        [IgnoreDataMember]
        private bool _HasAnyAllocated;
        [DataMember]
        public bool HasAnyAllocated
        {
            get => _HasAnyAllocated;
            set => _HasAnyAllocated = value;
        }

        #endregion

        #region Methods

        public void AttachComponent()
        {
            if (ComponentRef == null && ComponentInstance != null)
            {
                ComponentRef = new ACRef<IACComponent>(ComponentInstance, ACRef<IACComponent>.RefInitMode.AutoStartStop, false, null);
            }
        }

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
            return ComponentInstance != null ? ComponentInstance.ACUrl + "  :  " + Distance : "";
        }

        #endregion  
    }
}
