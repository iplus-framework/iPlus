// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace gip.core.datamodel
{
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RouteItem'}de{'RouteItem'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class RouteItem : ICloneable
    {
        [IgnoreDataMember]
        private ACClass _Source;
        [IgnoreDataMember]
        private ACClass _Target;
        [IgnoreDataMember]
        private ACClassProperty _SourceProperty;
        [IgnoreDataMember]
        private ACClassProperty _TargetProperty;

        #region Properties

        [IgnoreDataMember]
        public ACClass Source
        {
            get { return _Source; }
            private set
            {
                _Source = value;
                if (_Source != null)
                    _ACUrlSourceACComponent = _Source.GetACUrlComponent();
            }
        }

        [IgnoreDataMember]
        public ACClass Target
        {
            get { return _Target; }
            private set
            {
                _Target = value;
                if (_Target != null)
                    _ACUrlTargetACComponent = _Target.GetACUrlComponent();
            }
        }

        [IgnoreDataMember]
        public ACClassProperty SourceProperty
        {
            get { return _SourceProperty; }
            private set
            {
                _SourceProperty = value;
                if (_SourceProperty != null)
                    _ACIdentifierSourcePoint = _SourceProperty.ACIdentifier;
            }
        }

        [IgnoreDataMember]
        public ACClassProperty TargetProperty
        {
            get { return _TargetProperty; }
            private set
            {
                _TargetProperty = value;
                if (_TargetProperty != null)
                    _ACIdentifierTargetPoint = _TargetProperty.ACIdentifier;
            }
        }

        #region Serializable
        
        [IgnoreDataMember]
        private EntityKey _SourceKey = null;
        [DataMember]
        public EntityKey SourceKey
        {
            get
            {
                if (!IsAttached)
                {
                    if (_SourceKey != null)
                        return _SourceKey;
                }
                if (Source != null)
                    _SourceKey = Source.EntityKey;
                return _SourceKey;
            }
            set
            {
                _SourceKey = value;
            }
        }

        public Guid SourceGuid
        {
            get
            {
                return (Guid)SourceKey.EntityKeyValues.FirstOrDefault().Value;
            }
        }

        [IgnoreDataMember]
        private EntityKey _TargetKey = null;
        [DataMember]
        public EntityKey TargetKey
        {
            get
            {
                if (!IsAttached)
                {
                    if (_TargetKey != null)
                        return _TargetKey;
                }
                if (Target != null)
                    _TargetKey = Target.EntityKey;
                return _TargetKey;
            }
            set
            {
                _TargetKey = value;
            }
        }

        public Guid TargetGuid
        {
            get
            {
                return (Guid)TargetKey.EntityKeyValues.FirstOrDefault().Value;
            }
        }

        [IgnoreDataMember]
        private EntityKey _SourcePropertyKey = null;
        [DataMember]
        public EntityKey SourcePropertyKey
        {
            get
            {
                if (!IsAttached)
                {
                    if (_SourcePropertyKey != null)
                        return _SourcePropertyKey;
                }
                if (SourceProperty != null)
                    _SourcePropertyKey = SourceProperty.EntityKey;
                return _SourcePropertyKey;
            }

            set
            {
                _SourcePropertyKey = value;
            }
        }

        public Guid SourcePropertyGuid
        {
            get
            {
                return (Guid)_SourcePropertyKey.EntityKeyValues.FirstOrDefault().Value;
            }
        }


        [IgnoreDataMember]
        private EntityKey _TargetPropertyKey = null;
        [DataMember]
        public EntityKey TargetPropertyKey
        {
            get
            {
                if (!IsAttached)
                {
                    if (_TargetPropertyKey != null)
                        return _TargetPropertyKey;
                }
                if (TargetProperty != null)
                    _TargetPropertyKey = TargetProperty.EntityKey;
                return _TargetPropertyKey;
            }

            set
            {
                _TargetPropertyKey = value;
            }
        }

        public Guid TargetPropertyGuid
        {
            get
            {
                return (Guid)_TargetPropertyKey.EntityKeyValues.FirstOrDefault().Value;
            }
        }

        [DataMember]
        public int RouteNo
        {
            get;
            set;
        }

        [DataMember]
        public int? RouteItemWeight
        {
            get;
            set;
        }

        [DataMember]
        public Guid RelationID
        {
            get;
            set;
        }

        [DataMember]
        public bool IsDeactivated
        {
            get;
            set;
        }

        #endregion

#region ACComponents
        [IgnoreDataMember]
        private string _ACUrlSourceACComponent;
        [IgnoreDataMember]
        public IACComponent SourceACComponent
        {
            get
            {
                if (!String.IsNullOrEmpty(_ACUrlSourceACComponent))
                    return Database.Root.ACUrlCommand(_ACUrlSourceACComponent) as IACComponent;
                if (Source == null)
                    return null;
                _ACUrlSourceACComponent = Source.GetACUrlComponent();
                if (!String.IsNullOrEmpty(_ACUrlSourceACComponent))
                    return Database.Root.ACUrlCommand(_ACUrlSourceACComponent) as IACComponent;
                return null;
            }
        }

        [IgnoreDataMember]
        private string _ACIdentifierSourcePoint;
        [IgnoreDataMember]
        public IACPointBase SourceACPoint
        {
            get
            {
                IACComponent srcComp = SourceACComponent;
                if (srcComp == null)
                    return null;
                if (!String.IsNullOrEmpty(_ACIdentifierSourcePoint))
                    return srcComp.GetMember(_ACIdentifierSourcePoint) as IACPointBase;
                if (SourceProperty == null)
                    return null;
                _ACIdentifierSourcePoint = SourceProperty.ACIdentifier;
                if (!String.IsNullOrEmpty(_ACIdentifierSourcePoint))
                    return srcComp.GetMember(_ACIdentifierSourcePoint) as IACPointBase;
                return null;
            }
        }

        [IgnoreDataMember]
        private string _ACUrlTargetACComponent;
        [IgnoreDataMember]
        public IACComponent TargetACComponent
        {
            get
            {
                if (!String.IsNullOrEmpty(_ACUrlTargetACComponent))
                    return Database.Root.ACUrlCommand(_ACUrlTargetACComponent) as IACComponent;
                if (Target == null)
                    return null;
                _ACUrlTargetACComponent = Target.GetACUrlComponent();
                if (!String.IsNullOrEmpty(_ACUrlTargetACComponent))
                    return Database.Root.ACUrlCommand(_ACUrlTargetACComponent) as IACComponent;
                return null;
            }
        }

        [IgnoreDataMember]
        private string _ACIdentifierTargetPoint;
        [IgnoreDataMember]
        public IACPointBase TargetACPoint
        {
            get
            {
                IACComponent srcComp = TargetACComponent;
                if (srcComp == null)
                    return null;
                if (!String.IsNullOrEmpty(_ACIdentifierTargetPoint))
                    return srcComp.GetMember(_ACIdentifierTargetPoint) as IACPointBase;
                if (TargetProperty == null)
                    return null;
                _ACIdentifierTargetPoint = TargetProperty.ACIdentifier;
                if (!String.IsNullOrEmpty(_ACIdentifierTargetPoint))
                    return srcComp.GetMember(_ACIdentifierTargetPoint) as IACPointBase;
                return null;
            }
        }

#endregion

        [IgnoreDataMember]
        public bool IsAttached
        {
            get
            {
                return _Source != null && _SourceProperty != null && _Target != null && _TargetProperty != null;
            }
        }

        [IgnoreDataMember]
        public bool IsDetachedFromDBContext
        {
            get
            {
                if (_Source != null && _Source.Database.Entry(_Source).State != EntityState.Detached)
                    return false;
                if (_Target != null && _Target.EntityState != EntityState.Detached)
                    return false;
                if (_SourceProperty != null && _SourceProperty.EntityState != EntityState.Detached)
                    return false;
                if (_TargetProperty != null && _TargetProperty.EntityState != EntityState.Detached)
                    return false;
                return _Source == null && _SourceProperty == null && _Target == null && _TargetProperty == null;
            }
        }

        [IgnoreDataMember]
        public bool AreACUrlInfosSet
        {
            get
            {
                return !String.IsNullOrEmpty(_ACUrlSourceACComponent)
                    && !String.IsNullOrEmpty(_ACIdentifierSourcePoint)
                    && !String.IsNullOrEmpty(_ACUrlTargetACComponent)
                    && !String.IsNullOrEmpty(_ACIdentifierTargetPoint);
            }
        }

        #endregion

        #region Constructors

        public RouteItem(ACClassPropertyRelation relation, int routeNo=0, int? routeItemWeight = null)
        {
            if (relation == null) throw new ArgumentNullException("relation");

            Source = relation.SourceACClass;
            SourceProperty = relation.SourceACClassProperty;
            Target = relation.TargetACClass;
            TargetProperty = relation.TargetACClassProperty;
            RouteNo = routeNo;
            RouteItemWeight = routeItemWeight;
            RelationID = relation.ACClassPropertyRelationID;
        }

        public RouteItem(ACClass source, ACClassProperty sourceProperty, ACClass target, ACClassProperty targetProperty)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            else if (sourceProperty == null)
                throw new ArgumentNullException("sourceProperty");

            Source = source;
            SourceProperty = sourceProperty;
            Target = target;
            TargetProperty = targetProperty;
        }

        internal RouteItem()
        {
        }

#endregion

#region Methods

        public override string ToString()
        {
            return _Source.ACIdentifier + "." + _SourceProperty.ACIdentifier + " -> " + _Target.ACIdentifier + "." + _TargetProperty.ACIdentifier;      
        }

        public void AttachTo(IACEntityObjectContext context)
        {
            if (context == null)
                return;

            try
            {
                if (   _Source != null && _Source.Database == context
                    && _Target != null && _Target.Database == context
                    && _SourceProperty != null && _SourceProperty.Database == context
                    && _TargetProperty != null && _SourceProperty.Database == context)
                    return;
                Detach();
                object propValue;
                if (SourceKey != null && _Source == null)
                {
                    using (ACMonitor.Lock(context.ContextIPlus.QueryLock_1X000))
                    {
                        Source = context.ContextIPlus.GetObjectByKey(SourceKey) as ACClass;
                    }
                    if (Source != null)
                        propValue = SourceACComponent;
                }

                if (TargetKey != null && _Target == null)
                {
                    using (ACMonitor.Lock(context.ContextIPlus.QueryLock_1X000))
                    {
                        Target = context.ContextIPlus.GetObjectByKey(TargetKey) as ACClass;
                    }
                    if (Target != null)
                        propValue = TargetACComponent;
                }

                if (SourcePropertyKey != null && _SourceProperty == null)
                {
                    using (ACMonitor.Lock(context.ContextIPlus.QueryLock_1X000))
                    {
                        SourceProperty = context.ContextIPlus.GetObjectByKey(SourcePropertyKey) as ACClassProperty;
                    }
                    if (SourceProperty != null)
                        propValue = SourceACPoint;
                }

                if (TargetPropertyKey != null && _TargetProperty == null)
                {
                    using (ACMonitor.Lock(context.ContextIPlus.QueryLock_1X000))
                    {
                        TargetProperty = context.ContextIPlus.GetObjectByKey(TargetPropertyKey) as ACClassProperty;
                    }
                    if (TargetProperty != null)
                        propValue = TargetACPoint;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("RouteItem", "AttachTo", msg);
            }
        }

        public void Detach(bool detachFromContext = false)
        {
            if (detachFromContext)
                DetachEntitesFromDbContext();
            if (Source != null)
            {
                _SourceKey = Source.EntityKey;
                _Source = null;
            }
            if (Target != null)
            {
                _TargetKey = Target.EntityKey;
                _Target = null;
            }
            if (SourceProperty != null)
            {
                _SourcePropertyKey = SourceProperty.EntityKey;
                _SourceProperty = null;
            }
            if (TargetProperty != null)
            {
                _TargetPropertyKey = TargetProperty.EntityKey;
                _TargetProperty = null;
            }
        }

        public void DetachEntitesFromDbContext()
        {
            if (Source != null && Source.EntityState != EntityState.Detached)
                Source.Database.Entry(Source).State = EntityState.Detached;
                //Source.Database.Detach(Source);

            if (Target != null && Target.EntityState != EntityState.Detached)
                Target.Database.Entry(Target).State = EntityState.Detached;
                //Target.Database.Detach(Target);

            if (SourceProperty != null && SourceProperty.EntityState != EntityState.Detached)
                SourceProperty.Context.Entry(this).State = EntityState.Detached;
                //SourceProperty.Database.Detach(SourceProperty);

            if (TargetProperty != null && TargetProperty.EntityState != EntityState.Detached)
                TargetProperty.Context.Entry(this).State = EntityState.Detached;
                //TargetProperty.Database.Detach(TargetProperty);

        }

#endregion

        public override bool Equals(object obj)
        {
            bool isEqual = false;
            RouteItem routeItem = obj as RouteItem;
            if(routeItem != null)
                isEqual = this.SourceKey == routeItem.SourceKey && this.SourcePropertyKey == routeItem.SourcePropertyKey && 
                          this.TargetKey == routeItem.TargetKey && this.TargetPropertyKey == routeItem.TargetPropertyKey;
            return isEqual;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public object Clone()
        {
            return new RouteItem() { SourceKey = this.SourceKey,
                SourcePropertyKey = this.SourcePropertyKey,
                TargetKey = this.TargetKey,
                TargetPropertyKey = this.TargetPropertyKey,
                RouteNo = this.RouteNo,
                RelationID = this.RelationID,
                _ACUrlSourceACComponent = this._ACUrlSourceACComponent,
                _ACUrlTargetACComponent = this._ACUrlTargetACComponent,
                _ACIdentifierSourcePoint = this._ACIdentifierSourcePoint,
                _ACIdentifierTargetPoint = this._ACIdentifierTargetPoint};
        }
    }

    public class RouteItemSourceComparer : IEqualityComparer<RouteItem>
    {
        public bool Equals(RouteItem x, RouteItem y)
        {
            return x.SourceGuid == y.SourceGuid;
        }

        public int GetHashCode(RouteItem obj)
        {
            return obj.SourceGuid.GetHashCode();
        }
    }
}
