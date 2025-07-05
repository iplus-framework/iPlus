// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Base-Class for implementing Real- or Proxy-Implementations which holds the "wrapObjects"(Wrapper) in a local List.
    /// All in ACPointRefNetBase declared abstract methods operates on this local storage list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetBase'}de{'ACPointNetBase'}", Global.ACKinds.TACAbstractClass)]
    public abstract class ACPointNetBase<T, W> : IACObject, IACPointNet<T, W>
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetBase()
        {
            InitLockConnectionList();
            _ACRefParent = new ACRef<IACComponent>(this);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acTypeInfoProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetBase(IACComponent parent, IACType acTypeInfoProperty, uint maxCapacity)
        {
            InitLockConnectionList();
            _ACRefParent = new ACRef<IACComponent>(parent,this,true);
            _ACTypeInfo = acTypeInfoProperty;
            _MaxCapacity = maxCapacity;
            if (_ACRefParent.ValueT != null)
                (_ACRefParent.ValueT as ACComponent).AddOrReplaceACMember(this);
            if ((SetMethod == null) && (parent as ACComponent).ScriptEngine != null)
            {
                if ((parent as ACComponent).ScriptEngine.ExistsScript(ScriptTrigger.Type.OnSetACPoint, acTypeInfoProperty.ACIdentifier))
                    SetMethod = ((ACComponent)parent).OnSetACPoint;
            }
        }
        #endregion

        #region IACUrl
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
        public object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl,acParameter);
        }


        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl,acParameter);
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

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public string GetACUrlComponent(IACObject rootACObject = null)
        {
            return null;
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("","", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACRef.GetACUrl(rootACObject); 
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get;
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return _ACRefParent != null ? _ACRefParent.ValueT : null;
            }
        }  
        #endregion


        #region Own Member

        internal void DetachACObject()
        {
            _ACRefParent.Detach();
        }

        /*internal bool AttachACObject(IACObjectWithBinding acObject)
        {
            if (_ACComponent == null)
            {
                if (!String.IsNullOrEmpty(_ACObjectUrl))
                {
                    if (acObject.ACUrl == _ACObjectUrl)
                    {
                        _ACComponent = acObject;
                        return true;
                    }
                }
                else
                {
                    _ACComponent = acObject;
                    return true;
                }
            }
            return false;
        }*/

        #endregion

        #region IACPointNet<T,W> Member

        public abstract IEnumerable<T> RefObjectList { get; }

        public abstract W GetWrapObject(W cloneOrOriginal);

        public abstract W GetWrapObject(object cloneOrOriginal);

        public abstract bool Contains(W cloneOrOriginal);

        public abstract bool Contains(object cloneOrOriginal);

        public abstract bool Remove(W cloneOrOriginal);

        public abstract bool Remove(object cloneOrOriginal);

        public virtual bool MaxCapacityReached 
        {
            get
            {
                if (MaxCapacity <= 0)
                    return false;
                return true;
            }
        }

        #endregion

        #region IACPoint<W> Member

        /// <summary>
        /// List of ACPointNetWrapObject-relations to other components.
        /// </summary>
        public abstract IEnumerable<W> ConnectionList { get; }

        [ACPropertyInfo(800, "", "en{'Info about Connectionentries'}de{'Informationen über Einträge'}")]
        public virtual string ConnectionListInfo
        {
            get
            {
                try
                {
                    if (ConnectionList == null || !ConnectionList.Any())
                        return base.ToString() + "(Empty)";
                    StringBuilder sb = new StringBuilder();
                    int index = 0;
                    foreach (var entry in ConnectionList)
                    {
                        sb.AppendLine(String.Format("[{0}]: {1}", index, entry.ACCaption));
                        index++;
                    }
                    return sb.ToString();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPointNewBase<T,W>", "ConnectionListInfo", msg);
                }
                return base.ToString();
            }
        }

        #endregion

        #region IACPointBase Member

        [DataMember]
        protected uint _MaxCapacity = 0; // Infinite

        /// <summary>Maximum capacity of the point (of the ConnectionList). 0 = Unlimited</summary>
        /// <value>The maximum capacity.</value>
        [IgnoreDataMember]
        public uint MaxCapacity
        {
            get
            {
                return _MaxCapacity;
            }
        }

        private ACMonitorObject _20040_LockConnectionList = null;
        /// <summary>
        /// Lock for manipulation and querying of connectionlist
        /// </summary>
        public ACMonitorObject LockConnectionList_20040
        {
            get
            {
                if (_20040_LockConnectionList != null)
                    return _20040_LockConnectionList;
                InitLockConnectionList();
                return _20040_LockConnectionList;
            }
        }

        private void InitLockConnectionList()
        {
            if (_20040_LockConnectionList == null)
                _20040_LockConnectionList = new ACMonitorObject(20040);
        }

        /// <summary>Is called when the parent ACComponent is stopping/unloading.</summary>
        /// <param name="deleteACClassTask">if set to <c>true if the parent ACComponent should be removed from the persistable Application-Tree.</c></param>
        public abstract void ACDeInit(bool deleteACClassTask = false);

        /// <summary>The ConnectionList as serialized string (XML).</summary>
        /// <param name="xmlIndented">if set to <c>true</c> the XML is indented.</param>
        /// <returns>XML</returns>
        public abstract string ValueSerialized(bool xmlIndented = false);

        #endregion

        #region IACMember Member

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        public object Value
        {
            get
            {
                return this;
            }
            set
            {
                OnMemberChanged();
            }
        }


        /// <summary>
        /// Must be called inside the class that implements IACMember every time when the the encapsulated value-Property has changed.
        /// If the implementation implements INotifyPropertyChanged also then OnPropertyChanged() must be called inside the implementation of OnMemberChanged().
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.EventArgs"/> instance containing the event data. Is not null if the change of the encapsulated value was detected by a callback of the PropertyChangedEvent or CollectionChanged-Event. Then the EventArgs will be passed.
        /// </param>
        public void OnMemberChanged(EventArgs e = null)
        {
        }

        [IgnoreDataMember]
        protected IACType _ACTypeInfo;
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [IgnoreDataMember]
        public IACType ACType
        {
            get
            {
                if ((_ACTypeInfo == null) && !String.IsNullOrEmpty(_PropertyName))
                {
                    if (ACRef.ValueT != null)
                        _ACTypeInfo = ACRef.ValueT.ComponentClass.GetMember(_PropertyName);
                }

                return _ACTypeInfo;
            }
        }

        /// <summary>iPlus-Type (Metadata) of this point.</summary>
        /// <value>ACClassProperty</value>
        [IgnoreDataMember]
        public ACClassProperty PropertyInfo 
        {
            get
            {
                return ACType as ACClassProperty;
            }
        }


        /// <summary>Parent ACComponent where this instance belongs to.</summary>
        /// <value>The parent ac component.</value>
        public IACComponent ParentACComponent
        {
            get
            {
                if (!ACRef.IsObjLoaded)
                    return null;
                return ACRef.ValueT as ACComponent;
            }
        }


        /// <summary>
        /// This method is called from the iPlus-Framework for each member of a ACComponent when a component was recycled from the component-pool (ACInitState.RecycledFromPool) instead of a new creation.
        /// </summary>
        /// <param name="recycledComponent">The recycled component.</param>
        public void RecycleMemberAndAttachTo(IACComponent recycledComponent)
        {
            if (recycledComponent != null && _ACRefParent != null)
            {
                _ACRefParent.Value = recycledComponent;
            }
        }

        [IgnoreDataMember]
        protected string _PropertyName;

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [DataMember(Name = "CPN")]
        public string ACIdentifier
        {
            get
            {
                if (_ACTypeInfo == null)
                    return _PropertyName;
                return _ACTypeInfo.ACIdentifier;
            }
            set
            {
                if (_ACTypeInfo == null)
                    _PropertyName = value;
            }
        }

        [IgnoreDataMember]
        protected ACRef<IACComponent> _ACRefParent = null;
        /// <summary>Smart-Pointer to the Parent ACComponent where this instance belongs to.</summary>
        /// <value>The parent ac component.</value>
        [DataMember]
        public ACRef<IACComponent> ACRef
        {
            get
            {
                if (_ACRefParent == null)
                    _ACRefParent = new ACRef<IACComponent>(this);
                return _ACRefParent;
            }
            internal set
            {
                _ACRefParent = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void OnPropertyChangedLocalList()
        {
            OnPropertyChanged("ConnectionListLocal");
            OnPropertyChanged("RefObjectListLocal");
        }

        internal void OnPropertyChangedGlobalList()
        {
            OnPropertyChanged("ConnectionList");
            OnPropertyChanged("RefObjectList");
            OnPropertyChanged("ConnectionListInfo");
        }

        internal void OnPropertyChangedLists()
        {
            OnPropertyChangedLocalList();
            OnPropertyChangedGlobalList();
        }

        #endregion

        #region IACPointNetBase Member

        [IgnoreDataMember]
        private bool _PointChangedForBroadcast = false;

        [IgnoreDataMember]
        public bool PointChangedForBroadcast
        {
            get
            {
                return _PointChangedForBroadcast;
            }

            set
            {

                using (ACMonitor.Lock(LockConnectionList_20040))
                {
                    _PointChangedForBroadcast = value;
                }
            }
        }

        /// <summary>
        /// If set, then Method-Invocations on Points are called synchronous.
        /// The invoking Thread is blocked until the answer from the server comes back.
        /// </summary>
        [IgnoreDataMember]
        public bool SynchronousMode { get; set; }

        [IgnoreDataMember]
        internal ACObjectRMIWaitHandle _CurrentSyncRequest = null;

        [IgnoreDataMember]
        private int _SyncRequestID = 0;
        [DataMember]
        internal int SyncRequestID
        {
            get
            {
                if (_CurrentSyncRequest != null)
                    return _CurrentSyncRequest.RequestID;
                return _SyncRequestID;
            }
            set
            {
                _SyncRequestID = value;
            }
        }

        public virtual void Subscribe(bool force = true)
        {
        }

        public virtual void ReSubscribe()
        {
        }

        public virtual void UnSubscribe()
        {
        }

        public abstract void RebuildAfterDeserialization(object parentSubscrObject);

        public abstract void OnPointReceivedRemote(IACPointNetBase receivedPoint);

        /// <summary>
        /// Setter-Delegat-Methode von Assembly-ACObjekten (Callback)
        /// </summary>
        [IgnoreDataMember]
        protected ACPointSetMethod _SetMethod = null;
        [IgnoreDataMember]
        public ACPointSetMethod SetMethod
        {
            get
            {
                return _SetMethod;
            }

            set
            {
                _SetMethod = value;
            }
        }

        public virtual void CopyDataOfWrapObject(object cloneOrOriginal)
        {
        }

        public bool IsPersistable
        {
            get
            {
                if (PropertyInfo == null)
                    return false;
                return PropertyInfo.IsPersistable;
            }
        }
#endregion

        public event EventHandler AllEntriesRemoved;

        protected void OnAllEntriesRemoved()
        {
            if (AllEntriesRemoved != null)
                AllEntriesRemoved(this, new EventArgs());
        }
    }
}
