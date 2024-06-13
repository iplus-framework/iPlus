// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACRef.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading;
using System.Data.Objects.DataClasses;
using System.Data;
using System.Data.Objects;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACRef is a Class which manages the "SMART-Pointer" to an referenced ACComponent or Entity.
    /// ACRef should be used in this cases:<para />
    /// 
    /// --- 1.) For serialization of References ---<para />
    /// Properties of ACComponents which have a Reference to another ACComponent or to an Entity an should be Broadcastet over network.<para />
    /// 
    /// --- 2.) For Garbage-Collection of Proxy-Components ---<para />
    /// Unused Proxy-Components or Workflow-Proxies which were instantiated and subscribed at the Server for Broadcasting should
    /// be automatically unsubscribed and stopped. Every ACRef-Object which is a "Strong Reference" (the Property "IsWeakReference" is false) to an Target Component,
    /// has an Entry in the ACPointReference-List of the target component. When the ACRef gets detached, it automatically Removes the entry in the ACPointReference-List on the target component.
    /// When the ACPointReference-List has no entries with strong References any more, the Component will be marked for garbage collecting. After a certain time,
    /// the Proxy-Component will be stopped by the iPlus-Garbage-Collector and moved to the Component-Pool.<para />
    /// 
    /// --- 3.) Reloading of Components during runtime ---<para />
    /// When Scripts or Properties of a ACClass was changed during runtime, the corresponding components should be restarted on the server.
    /// A component could only be removed by the .NET-Garbage-Collector when it's not referenced by other Components. In this case use
    /// a ACRef-Object as a Property. When the Component will be stopped all ACRef-Objects which have a strong reference to this component will be automatically Detached.
    /// Then the component can be deleted from the .NET-Garbage-Collector. Instaed of the old component an new Instance will be created.
    /// Whe the "Obj"-Property in the ACRef-Object will be accessed, the ACRef-Object attaches the reference to the ne instantiated component.
    /// IMPLEMENTATION-RULES:<para />
    /// 
    /// --- Rule 1: "Unsubscribe Events of detached Component" ---<para />
    /// When you use ACRef for referencing an ACComponent and you have registered to an Event e.g. Property-Changed event,
    /// please unsubscribe these events when ACRef gets detached. Only then the .net-Garbage-Collector is possible to delete the Component.
    /// Afterwards when the ACRef gets attached you should Resubscribe to the new instantiated ACComponent.
    /// You can do this by subscribing to the ObjectDetached and ObjectAttached-event from the ACRef.<para />
    /// 
    /// --- Rule 2: "Detach ACRef in overidden ACDeInit-Method" ---<para />
    /// When a Component should be restartet and it has ACRef's as Members, all ACRef's must be Detached in the overidden ACDeInit-Method !!!
    /// </summary>
    /// <typeparam name="T">Generic type of the referenced object</typeparam>
    /// <seealso cref="gip.core.datamodel.IACContainerT{T}" />
    /// <seealso cref="System.ICloneable" />
    /// <seealso cref="gip.core.datamodel.IACContainerRef" />
    /// <seealso cref="System.IDisposable" />
    [DataContract]
    public class ACRef<T> : IACContainerT<T>, ICloneable, IACContainerRef, IDisposable where T : IACObject
    {
        #region enums
        /// <summary>
        /// Behaviour for automatic start and stop of the referenced component.
        /// </summary>
        public enum RefInitMode : short
        {
            NoInstantiation = 0,
            AutoStart = 1,
            AutoStop = 2,
            AutoStartStop = 3,
        }
        #endregion

        #region c´tors
        /// <summary>
        /// Internal Constructor. Don't use it.
        /// </summary>
        public ACRef()
        {
            _Mode = RefInitMode.AutoStart;
            InitLockRef();
        }

        /// <summary>
        /// Initializes a new ACRef.
        /// The referenced object will be set later.
        /// </summary>
        /// <param name="parentACObject">Reference to the parent component that holds this instance</param>
        public ACRef(IACObject parentACObject)
        {
            _Mode = RefInitMode.AutoStart;
            InitLockRef();
            this._ParentACObject = parentACObject;
        }

        /// <summary>
        /// Initializes a new ACRef via ACUrl
        /// </summary>
        /// <param name="acUrl">ACUrl of the component which should be referenced</param>
        /// <param name="parentACObject">Reference to the parent component that holds this instance.</param>
        /// <param name="autoAttach">if set to <c>true</c> the component with the passed ACUrl will be attached.</param>
        /// <param name="isWeakReference">if set to <c>true</c> the iPlus-Garbagecollector can stop the referenced component even if this instance has referenced it</param>
        public ACRef(string acUrl, IACObject parentACObject, bool autoAttach = false, bool isWeakReference = false)
            : this(acUrl, RefInitMode.NoInstantiation, parentACObject, autoAttach)
        {
        }

        /// <summary>
        /// Initializes a new ACRef via passed object
        /// </summary>
        /// <param name="acObject">Object (Entity or ACComponent) which should be referenced</param>
        /// <param name="parentACObject">Reference to the parent component that holds this instance.</param>
        public ACRef(T acObject, IACObject parentACObject)
            : this(acObject, RefInitMode.NoInstantiation, false, parentACObject)
        {
        }

        /// <summary>
        /// Initializes a new ACRef via passed object
        /// </summary>
        /// <param name="acObject">Object (Entity or ACComponent) which should be referenced</param>
        /// <param name="parentACObject">Reference to the parent component that holds this instance.</param>
        /// <param name="isWeakReference">if set to <c>true</c> the iPlus-Garbagecollector can stop the referenced component even if this instance has referenced it</param>
        public ACRef(T acObject, IACObject parentACObject, bool isWeakReference)
            : this(acObject, RefInitMode.NoInstantiation, isWeakReference, parentACObject)
        {
        }

        /// <summary>
        /// Initializes a new ACRef via passed object
        /// </summary>
        /// <param name="acObject">Object (Entity or ACComponent) which should be referenced</param>
        /// <param name="isWeakReference">if set to <c>true</c> the iPlus-Garbagecollector can stop the referenced component even if this instance has referenced it</param>
        public ACRef(T acObject, bool isWeakReference)
            : this(acObject, RefInitMode.NoInstantiation, isWeakReference, null)
        {
        }

        /// <summary>
        /// Initializes a new ACRef via ACUrl
        /// </summary>
        /// <param name="acUrl">ACUrl of the component which should be referenced</param>
        /// <param name="mode">Behaviour for automatic start and stop of the referenced component</param>
        /// <param name="parentACObject">Reference to the parent component that holds this instance.</param>
        /// <param name="autoAttach">if set to <c>true</c> the component with the passed ACUrl will be attached.</param>
        /// <param name="isWeakReference">if set to <c>true</c> the iPlus-Garbagecollector can stop the referenced component even if this instance has referenced it</param>
        public ACRef(string acUrl, RefInitMode mode, IACObject parentACObject, bool autoAttach = false, bool isWeakReference = false)
        {
            InitLockRef();
            this._ParentACObject = parentACObject;
            _ACUrl = acUrl;
            _Mode = mode;
            _IsWeakReference = isWeakReference;
            if (autoAttach)
                Attach();
        }

        /// <summary>
        /// Initializes a new ACRef via passed object
        /// </summary>
        /// <param name="acObject">Object (Entity or ACComponent) which should be referenced.</param>
        /// <param name="mode">Behaviour for automatic start and stop of the referenced component</param>
        /// <param name="isWeakReference">if set to <c>true</c> the iPlus-Garbagecollector can stop the referenced component even if this instance has referenced it</param>
        /// <param name="parentACObject">Reference to the parent component that holds this instance.</param>
        public ACRef(T acObject, RefInitMode mode, bool isWeakReference, IACObject parentACObject)
        {
            InitLockRef();
            this._ParentACObject = parentACObject;
            _Mode = mode;
            _IsWeakReference = isWeakReference;
            ValueT = acObject;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ACRef{T}"/> class.
        /// </summary>
        ~ACRef()
        {
            // TODO: Don't use code in Finalizer which handles critical sections
            // https://msdn.microsoft.com/de-de/library/b1yfkh5e(v=vs.110).aspx
            // https://social.msdn.microsoft.com/Forums/sqlserver/en-US/7a195ea3-e078-4a93-aba3-492e5b9313a8/about-the-finalizer-thread-and-threadsafety?forum=clr
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Detach(disposing == true ? 1 : 2, false);
        }

        #endregion

        #region Properties

        #region Serializable
        [IgnoreDataMember]
        protected string _ACUrl = "";
        /// <summary>
        /// ACUrl of the referenced Component.
        /// </summary>
        [DataMember]
        public string ACUrl
        {
            get
            {
                if (!IsAttached)
                {
                    if (_EntityKey != null)
                        return "";
                    if (!String.IsNullOrEmpty(_ACUrl))
                        return _ACUrl;
                }

                if (ValueT != null && !(ValueT is EntityObject))
                    _ACUrl = this.ValueT.GetACUrl();
                return _ACUrl;
            }
            set
            {
                _ACUrl = value;
            }
        }

        [IgnoreDataMember]
        private EntityKey _EntityKey = null;
        /// <summary>
        /// Entity-Key if EntityObject is referenced
        /// </summary>
        [DataMember]
        public EntityKey EntityKey
        {
            get
            {
                if (!IsAttached)
                {
                    if (_EntityKey != null)
                        return _EntityKey;
                    if (!String.IsNullOrEmpty(_ACUrl))
                        return null;
                }
                if (ValueT != null && ValueT is EntityObject)
                    _EntityKey = (ValueT as EntityObject).EntityKey;
                return _EntityKey;
            }

            set
            {
                _EntityKey = value;
            }
        }
        #endregion

        #region Referenced-Object

        [IgnoreDataMember]
        private ACMonitorObject _10030_LockRef;
        [IgnoreDataMember]
        public ACMonitorObject LockRef_10030
        {
            get
            {
                if (_10030_LockRef != null)
                    return _10030_LockRef;
                InitLockRef();
                return _10030_LockRef;
            }
        }

        private void InitLockRef()
        {
            if (_10030_LockRef == null)
                _10030_LockRef = new ACMonitorObject(10030);
        }

        [IgnoreDataMember]
        protected T _ValueT;
        /// <summary>Gets or sets the encapsulated value of the generic type T. 
        /// The Referenced object is a IACComponent or entity framework object. By accessing this Property the referenced object will automatically attached if its in detached state.</summary>
        /// <value>The Value-Property as generic type</value>
        [IgnoreDataMember]
        public T ValueT
        {
            get
            {
                Attach();
                return _ValueT;
            }
            set
            {
                if ((object)_ValueT != (object)value)
                {
                    Detach();
                    _ValueT = value;
                    if (_ValueT is IACComponent)
                    {
                        if ((_ValueT as IACComponent).ReferencePoint != null)
                            (_ValueT as IACComponent).ReferencePoint.Add(this);
                    }
                }
            }
        }


        /// <summary>Gets or sets the encapsulated value as a boxed type.
        /// Referenced object (component or entity object). By accessing this Property the referenced object will automatically attached if its in detached state.
        /// </summary>
        /// <value>The boxed value.</value>
        [IgnoreDataMember]
        public object Value
        {
            get
            {
                return ValueT;
            }
            set
            {
                ValueT = (T)value;
            }
        }


        /// <summary>
        /// Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        [IgnoreDataMember]
        public ACClass ValueTypeACClass
        {
            get
            {
                return ValueT == null ? null : ValueT.ACType as ACClass;
            }
        }


        /// <summary>Tests if object is referenced (attached).</summary>
        /// <value>
        ///   <c>true</c> if the referenced object are attached; otherwise, <c>false</c>.</value>
        [IgnoreDataMember]
        public bool IsAttached
        {
            get
            {
                if (_ValueT == null)
                    return false;
                return true;
            }
        }


        /// <summary>
        /// Tests if ValueT is not null. By accessing this Property the referenced object will automatically attached if its in detached state.
        /// </summary>
        [IgnoreDataMember]
        public bool IsObjLoaded
        {
            get
            {
                if (ValueT == null)
                    return false;
                return true;
            }
        }
        #endregion

        #region Miscellaneous and Garbage-Collector
        internal static void SetRoot(IRoot root)
        {
            if (_Root == null)
                _Root = root;
        }
        [IgnoreDataMember]
        private static IRoot _Root;
        [IgnoreDataMember]
        internal static IRoot Root
        {
            get
            {
                return _Root;
            }
        }


        [DataMember]
        private RefInitMode _Mode = RefInitMode.AutoStart;
        /// <summary>
        /// Behaviour for automatic start and stop of the referenced component.
        /// </summary>
        public RefInitMode Mode
        {
            get
            {
                return _Mode;
            }
        }

        [DataMember]
        protected bool _IsWeakReference;
        /// <summary>
        /// A weak reference is a reference that does not protect the referenced object from collection by the IPlus-GarbageCollector
        /// </summary>
        [IgnoreDataMember]
        public bool IsWeakReference
        {
            get
            {
                return _IsWeakReference;
            }
        }
        #endregion

        #region IACObject

        [IgnoreDataMember]
        IACObject _ParentACObject = null;
        /// <summary>
        /// Reference to the parent component that holds this instance.
        /// </summary>
        /// <value>Reference to the parent object</value>
        public virtual IACObject ParentACObject
        {
            get
            {
                return _ParentACObject;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>Returns NULL</value>
        [IgnoreDataMember]
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get
            {
                return null;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [IgnoreDataMember]
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get
            {
                if (!String.IsNullOrEmpty(ACUrl))
                    return ACUrl;
                else if (EntityKey != null)
                    return EntityKey.ToString();
                return null;
            }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [IgnoreDataMember]
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        [IgnoreDataMember]
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return new List<IACObject>() { ValueT };
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Attaching and Detaching
        /// <summary>
        /// Attaches the referenced object if its in detached state.
        /// </summary>
        public virtual void Attach()
        {
            if (   (_ValueT == null && (!String.IsNullOrEmpty(_ACUrl) || _EntityKey != null))
                || (_ValueT != null && _ValueT is EntityObject && (_ValueT as EntityObject).EntityState == EntityState.Detached))
            {
                T valueT;
                string acUrl;
                EntityKey entityKey;

                using (ACMonitor.Lock(LockRef_10030))
                {
                    valueT = _ValueT;
                    acUrl = _ACUrl;
                    entityKey = _EntityKey;
                }

                bool attachingDone = false;
                if ((valueT == null && (!String.IsNullOrEmpty(acUrl) || entityKey != null))
                    || (valueT != null && valueT is EntityObject && (valueT as EntityObject).EntityState == EntityState.Detached))
                {
                    attachingDone = true;
                    try
                    {
                        if (ACRef<IACObject>.Root.InitState == ACInitState.Destructed
                            || ACRef<IACObject>.Root.InitState == ACInitState.Destructing
                            || ACRef<IACObject>.Root.InitState == ACInitState.DisposedToPool
                            || ACRef<IACObject>.Root.InitState == ACInitState.DisposingToPool)
                            return;
                        IACObject context = null;
                        //if ((ParentACObject != null) && (ParentACObject is IACComponent))
                        //context = ParentACObject as IACComponent;
                        if (ParentACObject != null)
                            context = ParentACObject;
                        else
                            context = ACRef<IACObject>.Root;
                        if (context != null)
                        {
                            object result = null;
                            if (entityKey != null)
                            {
                                if (entityKey.EntityKeyValues == null || !entityKey.EntityKeyValues.Any())
                                    entityKey = null;
                                else
                                {
                                    try
                                    {
                                        IACEntityObjectContext database = context as IACEntityObjectContext;
                                        if (database != null)
                                        {

                                            using (ACMonitor.Lock(database.QueryLock_1X000))
                                            {
                                                if (valueT != null && (valueT as EntityObject).EntityState == EntityState.Detached)
                                                {
                                                    database.Attach(valueT as EntityObject);
                                                    result = valueT;
                                                }
                                                else
                                                    result = database.GetObjectByKey(entityKey);
                                            }
                                        }
                                        else
                                        {
                                            IACComponent component = context as IACComponent;
                                            if (component == null)
                                                component = ACRef<IACObject>.Root;
                                            if (component.Database.DefaultContainerName == entityKey.EntityContainerName)
                                            {

                                                using (ACMonitor.Lock(component.Database.QueryLock_1X000))
                                                {
                                                    if (valueT != null && (valueT as EntityObject).EntityState == EntityState.Detached)
                                                    {
                                                        component.Database.Attach(valueT as EntityObject);
                                                        result = valueT;
                                                    }
                                                    else
                                                        result = component.Database.GetObjectByKey(entityKey);
                                                }
                                            }
                                            else if (component.Database.ContextIPlus.DefaultContainerName == entityKey.EntityContainerName)
                                            {

                                                using (ACMonitor.Lock(component.Database.ContextIPlus.QueryLock_1X000))
                                                {
                                                    if (valueT != null && (valueT as EntityObject).EntityState == EntityState.Detached)
                                                    {
                                                        component.Database.ContextIPlus.Attach(valueT as EntityObject);
                                                        result = valueT;
                                                    }
                                                    else
                                                        result = component.Database.ContextIPlus.GetObjectByKey(entityKey);
                                                }
                                            }
                                            else
                                            {
                                                IACEntityObjectContext objectContext = ACObjectContextManager.Contexts.Where(c => c.DefaultContainerName == entityKey.EntityContainerName).FirstOrDefault();
                                                if (objectContext != null)
                                                {

                                                    using (ACMonitor.Lock(objectContext.QueryLock_1X000))
                                                    {
                                                        if (valueT != null && (valueT as EntityObject).EntityState == EntityState.Detached)
                                                        {
                                                            objectContext.Attach(valueT as EntityObject);
                                                            result = valueT;
                                                        }
                                                        else
                                                            result = objectContext.GetObjectByKey(entityKey);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        string msg = e.Message;
                                        if (e.InnerException != null && e.InnerException.Message != null)
                                            msg += " Inner:" + e.InnerException.Message;

                                        if (Database.Root != null && Database.Root.Messages != null)
                                            Database.Root.Messages.LogException("ACRef", "Attach", msg);

                                        entityKey = null;
                                    }
                                }
                            }
                            else
                            {
                                if ((_Mode == RefInitMode.AutoStart) || (_Mode == RefInitMode.AutoStartStop))
                                {
                                    if (acUrl == "\\" && context == ACRef<IACObject>.Root)
                                        result = ACRef<IACObject>.Root;
                                    else
                                        result = context.ACUrlCommand(acUrl);
                                }
                                else
                                {
                                    if (acUrl == "\\" && context == ACRef<IACObject>.Root)
                                        result = ACRef<IACObject>.Root;
                                    else
                                        result = context.ACUrlCommand("?" + acUrl);
                                }
                            }

                            if (result == null)
                                return;
                            if (!(result is T))
                                return;
                            valueT = (T)result;
                            //if (!_IsWeakReference && _Obj is IACComponent)
                            if (valueT is IACComponent)
                            {
                                IACComponent acComp = valueT as IACComponent;
                                if (acComp.InitState != ACInitState.Destructing
                                    && acComp.InitState != ACInitState.Destructed
                                    && acComp.InitState != ACInitState.DisposingToPool
                                    && acComp.InitState != ACInitState.DisposedToPool
                                    && acComp.ReferencePoint != null)
                                    acComp.ReferencePoint.Add(this);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACRef", "Attach(10)", msg);
                    }
                }

                if (attachingDone)
                {

                    using (ACMonitor.Lock(LockRef_10030))
                    {
                        _ValueT = valueT;
                        _ACUrl = acUrl;
                        EntityKey = entityKey;
                    }

                    if (ObjectAttached != null)
                        ObjectAttached(this, new EventArgs());
                }

            }
        }

        /// <summary>
        /// Detaches the Reference to the T-Object and stores the Reference-Information into ACUrl or EntityKey-Property
        /// </summary>
        /// <param name="detachFromContext">If set to true the referenced Entity-Object will be detached from its database-context as well</param>
        public void Detach(bool detachFromContext = false)
        {
            Detach(0, detachFromContext);
        }

        protected virtual void Detach(int isFinalizing, bool detachFromContext)
        {
            if (_ValueT != null)
            {
                T valueT;
                string acUrl;
                EntityKey entityKey;

                if (isFinalizing == 2) // Ohne lock weil der Garbage-Collector andere Threds selbst blockiert die Code in diesem Objekt ausführen wollen
                {
                    valueT = _ValueT;
                    acUrl = _ACUrl;
                    entityKey = _EntityKey;
                }
                else
                {

                using (ACMonitor.Lock(LockRef_10030))
                    {
                        valueT = _ValueT;
                        acUrl = _ACUrl;
                        entityKey = _EntityKey;
                    }
                }

                bool detachingDone = false;
                if (valueT != null)
                {
                    try
                    {
                        bool isShuttingDown = Database.Root != null && (Database.Root.InitState == ACInitState.Destructing || Database.Root.InitState == ACInitState.Destructed);
                        if (isFinalizing <= 1 && ObjectDetaching != null) // Kein auslösen von Detach-Ereginissen aufgrund von Deadlock-Gefahren weil man nicht weiß was der Nutzer implementiert hat
                            ObjectDetaching(this, new EventArgs());
                        if (isFinalizing <= 1 && entityKey == null)
                            acUrl = valueT.GetACUrl();
                        if (valueT != null && detachFromContext && valueT is EntityObject && !isShuttingDown)
                        {
                            EntityObject entity = valueT as EntityObject;
                            if (entity.EntityState != EntityState.Detached)
                            {
                                var dbContext = entity.GetObjectContext();
                                if (dbContext != null)
                                    dbContext.FullDetach(entity);
                            }
                        }
                        if (valueT is IACComponent)
                        {
                            //if (!_IsWeakReference && (_Obj as IACComponent).ReferenceList != null)
                            if ((valueT as IACComponent).ReferencePoint != null)
                                (valueT as IACComponent).ReferencePoint.Remove(this); // Risiko weil Remove ebenfalls ein Lock verwendet

                            if (_Mode == RefInitMode.AutoStartStop && isFinalizing <= 1 && !isShuttingDown) // Stoppen im Finalizer ist nicht erlaubt aufgrund hoher Gefahr von Deadlocks
                            {
                                IACComponent parent = (valueT as IACComponent).ParentACComponent;
                                if (parent != null)
                                    parent.StopComponent((valueT as IACComponent).ACIdentifier);
                            }
                        }
                        detachingDone = true;
                    }
                    catch (Exception e)
                    {
                        if (isFinalizing == 2 && Database.Root != null && Database.Root.Messages != null && Database.Root.InitState == ACInitState.Initialized)
                            Database.Root.Messages.LogException("ACRef", "Detach(2,false)", e.Message);
                    }
                }

                if (detachingDone && isFinalizing <= 1)
                {

                    using (ACMonitor.Lock(LockRef_10030))
                    {
                        _ValueT = default(T);
                        _ACUrl = acUrl;
                        //_EntityKey = entityKey;
                    }

                    if (isFinalizing <= 1 && ObjectDetached != null)  // Kein auslösen von Detach-Ereginissen aufgrund von Deadlock-Gefahren weil man nicht weiß was der Nutzer implementiert hat
                        ObjectDetached(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Attaches this ACRef-Instance to the Parent-Object that holds/owns this Smart-Pointer.
        /// For entity-objects pass the database-context instead of a ACComponent.
        /// </summary>
        /// <param name="parentACObject">Reference to the parent component that holds this instance. For entity-objects pass the database-context.</param>
        public void AttachTo(IACObject parentACObject)
        {
            bool isAttached = IsAttached;
            if (isAttached)
            {
                isAttached = false;
                Detach();
            }
            _ParentACObject = parentACObject;
            if (!IsAttached)
                Attach();
        }
        #endregion

        #region IACObject
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
        public virtual object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return null;
            // ACUrlCommand darf niemals weitergeleitet werden an Obj aufgrund von Rekursionsgefahr.
            // Es kann nicht unterscheiden werden wer der Aufrufer ist
            // Beispiel Objekt A hat ACRef als Member und Referenziert Objekt B
            // Objekt B hat in seiner ReferenceList die ACRef-Instanz. Würde Objekt B ein ACUrlCommand senden, würde er sich quasi selbst aufrufen!!!
            // Eine Klassische .NET-Klasse hat auch keine Information darüber welche Objekte sie referenzieren.
            // Da ACComponents nicht wissen von welchem grafischen Objekt sie referenziert werden, ist die einzige Möglichtkeit die ACUrlCommand-Methode
            // als "Kommunikationskanal". Das grafische Objekt im VisualTree hängen und Bindings zu ACComponents haben dürfen nie ACRef's verwendet werden
            // und machen auch keinen Sinn. ACRef's dienen als Smartpointer, damit der .NET-Garbagecollector heruntergefahrene (ACDeinit) ACComponents löschen kann.
            // ACRef's haben daher nur Ihren zweck bei Beziehungen zwischen ACComponents bzw. ACObjekts die keine grafischen Objekte sind.
            //if (!IsObjLoaded)
            //    return null;
            //if (acUrl == "!ACDeInit")
            //{
            //    Detach();
            //    return null;
            //}
            //return this.Obj.ACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public virtual bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return true;
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACUrl;
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
            return this.ValueT.ACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }
#endregion

        public void ChangeMode(RefInitMode mode)
        {
            _Mode = mode;
        }

        //#region Critical Section
        ///// <summary>
        ///// Locks the ref.
        ///// </summary>
        //public void LockRef()
        //{
        //    ACMonitor.Enter(_LockRef);
        //}

        ///// <summary>
        ///// Uns the lock ref.
        ///// </summary>
        //public void UnLockRef()
        //{
        //    ACMonitor.Exit(_LockRef);
        //}
        //#endregion

#endregion

#region Events

        /// <summary>
        /// Occurs when the referenced object was detached
        /// </summary>
        public event EventHandler ObjectDetached;

        /// <summary>
        /// Occurs before the deserialized content will be attached to be able to access the referenced object later.
        /// </summary>
        public event EventHandler ObjectDetaching;

        /// <summary>
        /// Occurs when  referenced object was attached.
        /// </summary>
        public event EventHandler ObjectAttached;
#endregion

        public object Clone()
        {
            ACRef<T> newRef = new ACRef<T>();
            newRef._ACUrl = this._ACUrl;
            newRef._EntityKey = this._EntityKey;
            newRef._IsWeakReference = this._IsWeakReference;
            newRef._Mode = this._Mode;
            newRef._ParentACObject = this._ParentACObject;
            newRef._ValueT = this._ValueT;
            return newRef;
        }
    }
}
