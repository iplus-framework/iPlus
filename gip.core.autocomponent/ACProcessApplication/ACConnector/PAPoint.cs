using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// A PAEdge represents a physical replationship between a Source-Connectionpoint and a Target-Connectionpoint.
    /// Therefore one PAEdge-Instance occurs in the Connectonslist of two Connectonpoints (Source and Target).
    /// </summary>
    [DataContract]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, "PAEdge", "en{'PAEdge'}de{'PAEdge'}", typeof(PAEdge), "","","")]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Relationship between two Connectionpoints'}de{'Beziehung zwischen zwei Verbindungspunkten'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PAEdge : IACObject, IACEdge, INotifyPropertyChanged
    {
        public const string ClassName = "PAEdge";

        public PAEdge(PAPoint target, PAPoint source, Guid relationID)
        {
            _Target = target;
            _Source = source;
            RelationID = relationID;
        }

        public PAEdge(PAPoint target, PAPoint source, ACClassPropertyRelation relation)
        {
            _Target = target;
            _Source = source;
            _Relation = relation;
            RelationID = _Relation.ACClassPropertyRelationID;
            Weight = _Relation.RelationWeight;
            IsDeactivated = _Relation.IsDeactivated;
            ACRoutingService.AddToEdgeCache(this, relation.UseFactor);
        }

        [DataMember]
        IACPointBase _Source;
        [IgnoreDataMember]
        public IACPointBase Source 
        {
            get
            {
                return _Source;
            }
            protected set { _Source = value; }
        }

        [DataMember]
        IACPointBase _Target;
        [IgnoreDataMember]
        public IACPointBase Target 
        {
            get
            {
                return _Target;
            }
            protected set { _Target = value; }
        }

        [IgnoreDataMember]
        ACClassPropertyRelation _Relation;
        [IgnoreDataMember]
        public ACClassPropertyRelation Relation
        {
            get
            {
                return _Relation;
            }
        }

        [DataMember]
        public Guid? RelationID
        {
            get;
            set;
        }

        [DataMember]
        private int _Weight;
        [ACPropertyInfo(999,"", "en{'Weight'}de{'Gewicht'}")]
        [IgnoreDataMember]
        public int Weight
        {
            get
            {
                return _Weight > 0 ? _Weight : 100;
            }
            set
            {
                _Weight = value;
            }
        }

        [DataMember]
        private bool _IsDeactivated;

        public event PropertyChangedEventHandler PropertyChanged;

        [ACPropertyInfo(999, "", "en{'Is deactivated'}de{'Ist deaktiviert'}")]
        [IgnoreDataMember]
        public bool IsDeactivated
        {
            get
            {
                return _IsDeactivated;
            }
            set
            {
                _IsDeactivated = value;
                OnPropertyChanged("IsDeactivated");
            }
        }

        [IgnoreDataMember]
        public int Delta
        {
            get { return this.Weight /* + this.Head.Distance - this.Tail.Distance*/; }
        }

        /// <summary>
        /// Tells if the edge is a possible sidetrack of specified vertex
        /// </summary>
        /// <param name="_v">Vertex to evaluate</param>
        /// <returns>True if edge is sidetrack of vertex, false if not</returns>
        public bool IsSidetrackOf(ACRoutingVertex _v)
        {
            return (this.SourceParent == _v.ComponentInstance && this != _v.EdgeToPath && this.Weight >= 0);
        }

        [IgnoreDataMember]
        Global.Directions Direction
        {
            get
            {
                return Relation.Direction;
            }
        }

        [IgnoreDataMember]
        [ACPropertyInfo(999,"","en{'Edge'}de{'Edge'}")]
        public string EdgeName
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            //return Source != null && Source.ParentACComponentRef != null && Source.ParentACComponentRef.ValueT != null && Target != null && Target.ParentACComponentRef != null &&
            //       Target.ParentACComponentRef.ValueT != null? 
            //       Source.ParentACComponentRef.ValueT.ACUrl_IfLink + " -> " + Target.ParentACComponentRef.ValueT.ACUrl_IfLink : "PAEdge";

            return Source != null && Source.ACRef != null && Source.ACRef.ValueT != null && Target != null && Target.ACRef != null &&
               Target.ACRef.ValueT != null ?
               Source.ACRef.ValueT.ACUrl + " -> " + Target.ACRef.ValueT.ACUrl : "PAEdge";
        }

        [IgnoreDataMember]
        public IACObject SourceParent
        {
            get
            {
                //return Source.ParentACComponentRef != null && Source.ParentACComponentRef.ValueT != null ? Source.ParentACComponentRef.ValueT : null;
                return Source.ACRef != null && Source.ACRef.ValueT != null ? Source.ACRef.ValueT : null;
            }
        }

        [IgnoreDataMember]
        public IACComponent SourceParentComponent
        {
            get => SourceParent as IACComponent;
        }

        [IgnoreDataMember]
        public IACObject TargetParent
        {
            get
            {
                //return Target.ParentACComponentRef != null && Target.ParentACComponentRef.ValueT != null ? Target.ParentACComponentRef.ValueT : null;
                return Target.ACRef != null && Target.ACRef.ValueT != null ? Target.ACRef.ValueT : null;
            }
        }

        [IgnoreDataMember]
        public IACComponent TargetParentComponent
        {
            get => TargetParent as IACComponent;
        }

        public BitAccessForAllocatedByWay GetAllocationState(bool isTarget)
        {
            if(!isTarget)
            {
                if (Source == null || Source.ACRef == null || Source.ACRef.ValueT == null)
                    return new BitAccessForAllocatedByWay();

                // If real instance:
                IRoutableModule routableModule = Source.ACRef.ValueT as IRoutableModule;
                if (routableModule != null && routableModule.AllocatedByWay != null)
                    return routableModule.AllocatedByWay.ValueT;

                // Else proxy:
                IACMember member = Source.ACRef.ValueT.GetMember(nameof(IRoutableModule.AllocatedByWay));
                if (member == null || member.Value == null || !(member.Value is BitAccessForAllocatedByWay))
                    return new BitAccessForAllocatedByWay();

                return (BitAccessForAllocatedByWay)member.Value;
            }
            else
            {
                if (Target == null || Target.ACRef == null || Target.ACRef.ValueT == null)
                    return new BitAccessForAllocatedByWay();
                
                // If real instance:
                IRoutableModule routableModule = Target.ACRef.ValueT as IRoutableModule;
                if (routableModule != null && routableModule.AllocatedByWay != null)
                    return routableModule.AllocatedByWay.ValueT;

                // Else proxy:
                IACMember memberTarget = Target.ACRef.ValueT.GetMember(nameof(IRoutableModule.AllocatedByWay));
                if (memberTarget == null || memberTarget.Value == null || !(memberTarget.Value is BitAccessForAllocatedByWay))
                    return new BitAccessForAllocatedByWay();

                return (BitAccessForAllocatedByWay)memberTarget.Value;
            }
        }

        public void AttachRelation(Database db)
        {
            _Relation = db.ACClassPropertyRelation.FirstOrDefault(c => c.ACClassPropertyRelationID == RelationID);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region IACObject
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get => ACIdentifier;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get => this.ReflectACType();
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get => this.ReflectGetACContentList();
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return Source; }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ReflectGetACUrl(rootACObject);
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.ReflectGetACIdentifier(); }
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }
        #endregion
    }



    /// <summary>
    /// Connectionpoint that represent physical relationships between components.
    /// The physical relationships are configured in the development enviroment and stored in the database-table ACClassPropertyRelation.
    /// The ACPostInit()-Method reads ACClassPropertyRelation (where ConnectionTypeIndex == Global.ConnectionTypes.ConnectionPhysical) and creates instances of PAEdge for each relation.
    /// PAEdges are stored in the ConnectionList.
    /// One PAEdge-Instance exists always in two different Connectionlists. One in a PAPoint, that represents a source and one in another PAPoint, that represents a target.
    /// This two-way connection allows you to define a network of physical paths in which you can search for or navigate targets in both directions.
    /// This pathfinding algorithms are implemented in the class ACRoutingService which uses this kind of Connectionpoints.
    /// </summary>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Connectionpoint for physical paths'}de{'Verbindungspunkt für physikalische Wege'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PAPoint : ACPointBase<PAEdge>
    {
        List<PAEdge> _PAEdges = new List<PAEdge>();

        public PAPoint(IACComponent parent, string acPropertyName)
            : base(parent, acPropertyName)
        {
        }

        public PAPoint(IACComponent parent, string acPropertyName, uint maxCapacity)
            : base(parent, acPropertyName, maxCapacity)
        {
        }

        /// <summary>
        /// Constructor for automatic Instantiation over Reflection in ACInitACPoint()
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public PAPoint(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
        }

        public virtual void ACPostInit()
        {
            if (_PAEdges == null)
                _PAEdges = new List<PAEdge>();
            foreach (ACClassPropertyRelation relation in this.PropertyInfo.TopBaseACClassProperty.GetMySourceConnectionPhysicalList(ParentACComponent.ACType as ACClass))
            {
                IACComponent targetACComponent = (IACComponent)ParentACComponent.ACUrlCommand(relation.TargetACClass.GetACUrlComponent());
                if (targetACComponent != null)
                {
                    PAPoint acPointTarget = targetACComponent.GetPoint(relation.TargetACClassProperty.ACIdentifier) as PAPoint;
                    if (acPointTarget != null && acPointTarget.ConnectionList != null)
                    {
                        PAEdge edge = new PAEdge(acPointTarget, this, relation);
                        _PAEdges.Add(edge);
                        (acPointTarget.ConnectionList as List<PAEdge>).Add(edge);
                    }
                }
            }
        }

        /// <summary>Is called when the parent ACComponent is stopping/unloading.</summary>
        /// <param name="deleteACClassTask">if set to <c>true if the parent ACComponent should be removed from the persistable Application-Tree.</c></param>
        public override void ACDeInit(bool deleteACClassTask = false)
        {
            _PAEdges = null;
            base.ACDeInit(deleteACClassTask);
        }

        #region IACConnectionPoint<ACClassPropertyRelation> Member
        public override IEnumerable<PAEdge> ConnectionList
        {
            get { return _PAEdges; }
        }
        #endregion

    }
}
