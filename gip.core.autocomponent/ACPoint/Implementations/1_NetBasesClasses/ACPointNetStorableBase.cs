using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Threading;
using gip.core.datamodel;
using System.Text.RegularExpressions;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Base-Class for implementing Real- or Proxy-Implementations which holds the "wrapObjects"(Wrapper) in a local List.
    /// All in ACPointRefNetBase declared abstract methods operates on this local storage list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetStorableBase'}de{'ACPointNetStorableBase'}", Global.ACKinds.TACAbstractClass)]
    public abstract class ACPointNetStorableBase<T, W> : ACPointNetBase<T, W>
        where W : ACPointNetWrapObject<T>
        where T : IACObject
    {

        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetStorableBase()
        {
            InitLockLocalStorage();
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetStorableBase(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
            InitLockLocalStorage();
        }

        /// <summary>Is called when the parent ACComponent is stopping/unloading.</summary>
        /// <param name="deleteACClassTask">if set to <c>true if the parent ACComponent should be removed from the persistable Application-Tree.</c></param>
        public override void ACDeInit(bool deleteACClassTask = false)
        {
            if (deleteACClassTask)
            {
                ACClassTaskValue taskValue = null;

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    taskValue = _ACClassTaskValue;
                    _ACClassTaskValue = null;
                }
                if (taskValue != null)
                {
                    ACClassTaskQueue.TaskQueue.Add(() =>
                    {
                        try
                        {
                            if (taskValue.EntityState != System.Data.EntityState.Deleted
                                && taskValue.EntityState != System.Data.EntityState.Detached)
                                ACClassTaskQueue.TaskQueue.Context.Detach(taskValue);
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("ACPointNetStorableBase<T, W>", "ACDeInit", msg);
                        }
                    }
                    );
                }
            }
        }

#endregion

#region Protected Member
        [IgnoreDataMember]
        protected List<W> _LocalStorage;

        [DataMember]
        public List<W> LocalStorage
        {
            get
            {
                return _LocalStorage;
            }
            internal set
            {
                _LocalStorage = value;
            }
        }

        private ACMonitorObject _20033_LockLocalStorage = null;
        /// <summary>
        /// Lock for manipulation and querying of connectionlist
        /// </summary>
        public ACMonitorObject LockLocalStorage_20033
        {
            get
            {
                if (_20033_LockLocalStorage != null)
                    return _20033_LockLocalStorage;
                InitLockLocalStorage();
                return _20033_LockLocalStorage;
            }
        }

        private void InitLockLocalStorage()
        {
            if (_20033_LockLocalStorage == null)
                _20033_LockLocalStorage = new ACMonitorObject(20033);
        }


        [IgnoreDataMember]
        private Nullable<bool> _IsComparableType = null;
        /// <summary>
        /// Returns if wrapped "refObject" implements the IComparable-Interface
        /// </summary>
        /// <returns></returns>
        protected bool IsComparableType()
        {
            if (_IsComparableType.HasValue)
                return _IsComparableType.Value;
            Type typeOfProperty = this.GetType();
            Type interfaceType = null;
            if (typeOfProperty.IsGenericType)
            {
                Type genericType = typeOfProperty.GetGenericArguments()[0];
                interfaceType = genericType.GetInterface("IComparable");
            }
            else
                interfaceType = typeOfProperty.GetInterface("IComparable");
            if (interfaceType != null)
                _IsComparableType = true;
            else
                _IsComparableType = false;
            return _IsComparableType.Value;
        }

        internal virtual bool AddToList(W wrapObject)
        {
            if ((_LocalStorage == null) || (wrapObject == null))
                return false;

            if (!Contains(wrapObject))
            {
                if (MaxCapacityReached)
                    return false;

                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    _LocalStorage.Add(wrapObject);
                }
                return true;
            }
            return false;
        }

        protected virtual void OnLocalStorageListChanged()
        {
        }

        /// <summary>
        /// If Persistable Property, then Load from Database
        /// </summary>
        /// <returns>Returns true if Value found in Database</returns>
        internal virtual bool ReStoreFromDB()
        {
            if (PropertyInfo == null)
                return false;
            // Gilt für Proxy- und Real-Points
            if (!PropertyInfo.IsPersistable 
                || !ACRef.IsObjLoaded 
                || (ACRef.ValueT.ContentTask == null)
                || (ACRef.ValueT.ACOperationMode == ACOperationModes.Test))
                return false;

            bool restored = false;
            string valueXML = null;
            try
            {
                ACClassTaskValue acClassTaskValue = null;

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    acClassTaskValue = _ACClassTaskValue;
                }

                if (acClassTaskValue == null)
                {
                    Guid vbUserID = ACRef.ValueT.Root.Environment.User.VBUserID;
                    Guid acClassPropertyID = ACType.ACTypeID;
                    Guid acClassTaskID = ACRef.ValueT.ContentTask.ACClassTaskID;
                    if (ACClassTaskQueue.TaskQueue.MassLoadPropertyValuesOff)
                    {
                        ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                        {
                            acClassTaskValue = ACClassTaskQueue.TaskQueue.Context.ACClassTaskValue
                            .Where(c =>    (c.ACClassPropertyID == acClassPropertyID) 
                                        && (c.VBUser != null) && (c.VBUserID == vbUserID) 
                                        && (c.ACClassTaskID == acClassTaskID))
                            .FirstOrDefault();
                        });
                    }
                    else
                    {
                        acClassTaskValue = ACClassTaskQueue.TaskQueue.GetFromAllPropValues(acClassTaskID, acClassPropertyID, vbUserID);
                    }


                    using (ACMonitor.Lock(_20015_LockValue))
                    {
                        if (_ACClassTaskValue == null && acClassTaskValue != null)
                            _ACClassTaskValue = acClassTaskValue;
                    }
                }

                if (acClassTaskValue != null)
                    ACClassTaskQueue.TaskQueue.ProcessAction(() => { valueXML = acClassTaskValue.XMLValue; });

                if (!String.IsNullOrEmpty(valueXML))
                {
                    using (StringReader ms = new StringReader(valueXML))
                    using (XmlTextReader xmlReader = new XmlTextReader(ms))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(List<W>), ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                        _LocalStorage = (List<W>)serializer.ReadObject(xmlReader);
                    }

                    using (ACMonitor.Lock(LockLocalStorage_20033))
                    {
                        if (_LocalStorage != null)
                        {
                            if (_LocalStorage.Any())
                                _LocalStorage.ForEach(c => c.Point = this);
                        }
                        restored = true;
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPointNetStorableBase<T, W>", "ReStoreFromDB", msg);
            }
            return restored;
        }

        public readonly ACMonitorObject _20015_LockValue = new ACMonitorObject(20015);
        protected ACClassTaskValue _ACClassTaskValue = null;
        // Locked access to _ACClassTaskValue
        public ACClassTaskValue ACClassTaskValue
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _ACClassTaskValue;
                }
            }
            internal set
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _ACClassTaskValue = value;
                }
            }
        }

        internal ACClassTaskValue ACClassTaskValueWithInit
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_ACClassTaskValue != null)
                        return _ACClassTaskValue;
                }
                bool added = false;

                IACComponent component = ACRef.ValueT;
                if (component == null)
                    return null;
                ACClassTask acClassTask = component.ContentTask;
                if (acClassTask == null)
                    return null;
                Guid thisACTypeID = ACType.ACTypeID;
                Guid vbUserID = component.Root.Environment.User.VBUserID;
                Guid componentClassID = component.ComponentClass.ACClassID;


                using (ACMonitor.Lock(_20015_LockValue))
                {
                    // Sofort ausführend, damit Instanz angelegt wird
                    ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        _ACClassTaskValue = ACRef.ValueT.ContentTask.ACClassTaskValue_ACClassTask
                                    .Where(c => (c.ACClassPropertyID == thisACTypeID)
                                            && c.VBUserID.HasValue
                                            && c.VBUserID == vbUserID)
                                    .FirstOrDefault();
                        if (_ACClassTaskValue == null)
                        {
                            ACClassProperty acClassProperty = ACClassTaskQueue.TaskQueue.GetACClassPropertyFromTaskQueueCache(thisACTypeID);
                            //var queryClass = RootDbOpQueue.ACClassTaskQueue.Context.ACClass.Where(c => c.ACClassID == componentClassID).FirstOrDefault();
                            if (acClassProperty != null)
                            {
                                _ACClassTaskValue = ACClassTaskValue.NewACClassTaskValue(ACClassTaskQueue.TaskQueue.Context, acClassTask, acClassProperty);
                                if (_UserFromPropertyContext == null)
                                    _UserFromPropertyContext = ACClassTaskQueue.TaskQueue.Context.VBUser.Where(c => c.VBUserID == vbUserID).FirstOrDefault();
                                _ACClassTaskValue.VBUser = _UserFromPropertyContext;
                                acClassTask.ACClassTaskValue_ACClassTask.Add(_ACClassTaskValue);
                                added = true;
//#if DEBUG
//                                    if (!this.ParentACComponent.Root.Initialized)
//                                    {
//                                        string acUrlSubscr = this.ParentACComponent.GetACUrl();
//                                        this.ParentACComponent.Messages.LogError(acUrlSubscr, this.ACIdentifier, String.Format("***Point*** _ACClassTaskValue {0} at ACClassTakID {1} generated during Restart", _ACClassTaskValue.ACClassTaskValueID, ACRef.ValueT.ContentTask.ACClassTaskID));
//                                    }
//#endif
                            }
                        }
                    });

                    if (added)
                    {
                        // Eintrag in Queue, Speicherung kann verzögert erfolgen.
                        ACClassTaskQueue.TaskQueue.Add(() => 
                            {
                                component.ContentTask.ACClassTaskValue_ACClassTask.Add(_ACClassTaskValue);
                            }
                        );
                    }
                    return _ACClassTaskValue;
                }
            }
        }

        protected static VBUser _UserFromPropertyContext = null;


        /// <summary>The ConnectionList as serialized string (XML).</summary>
        /// <param name="xmlIndented">if set to <c>true</c> the XML is indented.</param>
        /// <returns>XML</returns>
        public override string ValueSerialized(bool xmlIndented = false)
        {
            string valueXML = "";
            try
            {
                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    if (_LocalStorage.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        using (StringWriter sw = new StringWriter(sb))
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                        {
                            if (xmlIndented)
                                xmlWriter.Formatting = Formatting.Indented;
                            DataContractSerializer serializer = new DataContractSerializer(typeof(List<W>), ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                            serializer.WriteObject(xmlWriter, _LocalStorage);
                            valueXML = sw.ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPointNetStorableBase<T, W>", "ValueSerialized", msg);
            }
            return valueXML;
        }

        /// <summary>
        /// If Persistable Property, then store to Database
        /// </summary>
        /// <returns>Returns true if persistable property</returns>
        public virtual bool Persist(bool withLock)
        {
            if (!PropertyInfo.IsPersistable
                || !ACRef.IsObjLoaded 
                || (ACRef.ValueT.ContentTask == null)
                || (ACRef.ValueT.ACOperationMode == ACOperationModes.Test))
                return false;

            //if (withLock)
            //    ACRef.LockRef();
            try
            {
                string valueXML = ValueSerialized();
                string acUrl = this.ParentACComponent.GetACUrl();
                string acIdentifer = this.ACIdentifier;
                //#if DEBUG
                //                if (_ACClassTaskValue != null)
                //                {

                //                    if (!String.IsNullOrEmpty(_ACClassTaskValue.XMLValueRW)
                //                        && !String.IsNullOrEmpty(valueXML) 
                //                        && valueXML.Length < _ACClassTaskValue.XMLValueRW.Length
                //                        && (this is ACPointEvent || this is PWPointIn))
                //                    {
                //                        if (Regex.Match(valueXML, "<ACUrl").Groups.Count < Regex.Match(_ACClassTaskValue.XMLValueRW, "<ACUrl").Groups.Count)
                //                        {
                //                            this.ParentACComponent.Messages.LogErrorXML(acUrl, "Old:" + _ACClassTaskValue.XMLValueRW + "New:" + valueXML, acIdentifer, String.Format("***Point*** Connectionlist serialized with smaller count at ACClassTaskValueID {0}, ACClassTask: {1}", _ACClassTaskValue.ACClassTaskValueID, ACRef.ValueT.ContentTask.ACClassTaskID));
                //                        }
                //                    }
                //                }
                //#endif

                ACClassTaskValue acClassTaskValue = ACClassTaskValueWithInit;
                if (acClassTaskValue != null)
                {
                    ACClassTaskQueue.TaskQueue.Add(() =>
                        {
                            if (acClassTaskValue != null &&
                                acClassTaskValue.EntityState != System.Data.EntityState.Deleted
                                && acClassTaskValue.EntityState != System.Data.EntityState.Detached)
                            {
                                //#if DEBUG
                                //                                if (!String.IsNullOrEmpty(_ACClassTaskValue.XMLValue) && valueXML.Length < _ACClassTaskValue.XMLValue.Length)
                                //                                {
                                //                                    if (System.Diagnostics.Debugger.IsAttached)
                                //                                        System.Diagnostics.Debugger.Break();
                                //                                }

                                //                                if (System.Diagnostics.Debugger.IsAttached)
                                //                                {
                                //                                    this.ParentACComponent.Messages.LogDebugXML(acUrl, valueXML, acIdentifer, "Persist(2)");
                                //                                }
                                //#endif
                                acClassTaskValue.XMLValue = valueXML;
                            }
                        }
                    );
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPointNetStorableBase<T, W>", "Persist", msg);
            }
            return true;
        }

#endregion

#region IACConnectionPointBase Member

        public void UpdateLocalStorage(IACPointNetBase receivedPoint)
        {
            if (receivedPoint == null)
                return;
            ACPointNetStorableBase<T, W> receivedStorablePoint = (ACPointNetStorableBase<T, W>)receivedPoint;
            foreach (W receivedWrapObject in receivedStorablePoint.LocalStorage)
            {
                W localWrapObject = GetWrapObject(receivedWrapObject);
                if (localWrapObject != null)
                {
                    receivedWrapObject.CompareChangedData(localWrapObject);
                    //localWrapObject.CompareChangedData(receivedWrapObject);
                }
            }


            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                if (_LocalStorage != null)
                {
                    foreach (W oldWrapObject in _LocalStorage)
                    {
                        oldWrapObject.Detach();
                    }
                }
                _LocalStorage = receivedStorablePoint.LocalStorage;

                if (_LocalStorage != null && _LocalStorage.Any())
                {
                    _LocalStorage.ForEach(c => c.Point = this);
                }
            }
        }

        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint"></param>
        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
        }

        #endregion

        #region IACConnectionPoint<W> Member

        /// <summary>
        /// List of ACPointNetWrapObject-relations to other components.
        /// </summary>
        public override IEnumerable<W> ConnectionList
        {
            get
            {
                return _LocalStorage;
            }
        }

#endregion

#region IACPointRefNet<T,W> Member


        [IgnoreDataMember]
        public override IEnumerable<T> RefObjectList
        {
            get
            {
                if (_LocalStorage == null)
                    return null;
                return _LocalStorage.Select(c => c.ValueT);
            }
        }


        [IgnoreDataMember]
        internal ACPSubscrObjService ParentSubscrObject
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a "WrapObject". LocalStorage is search by two steps:
        /// 1. Lookup by comparing Object-Instance (Pointer). If Not succeed, step 2:
        /// 2. Comparing is done over IComparable-Interface by comparing Properties
        /// </summary>
        /// <param name="cloneOrOriginal">A wrapObject which is the Original Instance or a clone with same Properties</param>
        /// <returns>Returns a "WrapObject"</returns>
        public override W GetWrapObject(W cloneOrOriginal)
        {
            if ((_LocalStorage == null) || (cloneOrOriginal == null))
                return null;

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                if (_LocalStorage.Contains(cloneOrOriginal))
                    return cloneOrOriginal;
                return _LocalStorage.Where(c => ((W)c).CompareTo(cloneOrOriginal) == 0).FirstOrDefault();
            }
        }

        public override W GetWrapObject(object cloneOrOriginal)
        {
            if ((_LocalStorage == null) || (cloneOrOriginal == null))
                return null;
            if (cloneOrOriginal is W)
                return GetWrapObject((W)cloneOrOriginal);

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                return _LocalStorage.Where(c => ((W)c).CompareTo(cloneOrOriginal) == 0).FirstOrDefault();
            }
        }

        /// <summary>
        /// Checks if a "wrapObject" exists in the local storage List
        /// </summary>
        /// <param name="cloneOrOriginal">A wrapObject which is the Original Instance or a clone with same Properties</param>
        /// <returns></returns>
        public override bool Contains(W cloneOrOriginal)
        {
            if ((_LocalStorage == null) || (cloneOrOriginal == null))
                return false;
            W wrapObject = GetWrapObject(cloneOrOriginal);
            if (wrapObject == null)
                return false;
            return true;
        }

        public override bool Contains(object cloneOrOriginal)
        {
            if ((_LocalStorage == null) || (cloneOrOriginal == null))
                return false;
            W wrapObject = GetWrapObject(cloneOrOriginal);
            if (wrapObject == null)
                return false;
            return true;
        }

        /// <summary>
        /// Removes a "wrapObject" from the local storage list including it's wrapped "refObject" 
        /// </summary>
        /// <param name="cloneOrOriginal">A wrapObject which is the Original Instance or a clone with same Properties</param>
        /// <returns>returns true if object existed and was removed</returns>
        public override bool Remove(W cloneOrOriginal)
        {
            if (cloneOrOriginal == null)
                return false;
            if (!Contains(cloneOrOriginal))
                return false;
            bool removed = false;

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                removed = Convert.ToBoolean(_LocalStorage.RemoveAll(c => ((W)c).CompareToAtRemove(cloneOrOriginal) == 0));
                if (removed && _LocalStorage.Count <= 0)
                    OnAllEntriesRemoved();
            }
            if (removed)
                OnLocalStorageListChanged();
            return removed;
        }

        public override bool Remove(object cloneOrOriginal)
        {
            if (cloneOrOriginal == null)
                return false;
            if (!Contains(cloneOrOriginal))
                return false;
            bool removed = false;
            bool allEntriesRemoved = false;

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                removed = Convert.ToBoolean(_LocalStorage.RemoveAll(c => ((W)c).CompareToAtRemove(cloneOrOriginal) == 0));
                if (removed && _LocalStorage.Count <= 0)
                    allEntriesRemoved = true;
            }
            if (allEntriesRemoved)
                OnAllEntriesRemoved();
            if (removed)
                OnLocalStorageListChanged();
            return removed;
        }

        public override bool MaxCapacityReached
        {
            get
            {
                if (MaxCapacity <= 0)
                    return false;
                if (_LocalStorage == null)
                    return true;
                if (_LocalStorage.Count >= MaxCapacity)
                    return true;
                return false;
            }
        }

        public List<W> RejectedList
        {
            get
            {
                if (_LocalStorage == null)
                    return new List<W>();
                if (_LocalStorage.Count <= 0)
                    return new List<W>();

                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    var query = _LocalStorage.Where(c => c.State == PointProcessingState.Rejected);
                    if (!query.Any())
                        return new List<W>();
                    return query.ToList();
                }
            }
        }

        internal virtual int RemoveAllRejected(bool withLock)
        {
            List<W> rejectedList = RejectedList;
            if (rejectedList.Count <= 0)
                return 0;

            int countRemoved = 0;

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                foreach (W wrapObject in rejectedList)
                {
                    if (!String.IsNullOrEmpty(wrapObject.ClientPointName))
                    {
                        if (wrapObject.IsObjLoaded)
                        {
                            if (wrapObject.ValueT is ACComponent)
                            {
                                IACPointNetBase clientPoint = (wrapObject.ValueT as ACComponent).GetPointNet(wrapObject.ClientPointName);
                                if (clientPoint != null)
                                {
                                    if (clientPoint.Contains(wrapObject))
                                    {
                                        clientPoint.Remove(wrapObject);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (W wrapObject in rejectedList)
                {
                    if (_LocalStorage.RemoveAll(c => ((W)c).CompareToAtRemove(wrapObject) == 0) > 0)
                    {
                        countRemoved++;
                        if (_LocalStorage.Count <= 0)
                            OnAllEntriesRemoved();
                    }
                }
            }

            if (countRemoved > 0)
                OnLocalStorageListChanged();
            return countRemoved;
        }

        internal virtual int RemoveAll()
        {
            int countRemoved = LocalStorage.Count;
            if (countRemoved > 0)
            {

                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    _LocalStorage = new List<W>();
                }

                OnLocalStorageListChanged();
            }
            return countRemoved;
        }

        internal void CopyDataOfLocalStorageToClientPoints(bool withLock)
        {

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                foreach (W wrapObject in LocalStorage)
                {
                    if (!String.IsNullOrEmpty(wrapObject.ClientPointName))
                    {
                        if (wrapObject.IsObjLoaded)
                        {
                            if (wrapObject.ValueT is ACComponent)
                            {
                                IACPointNetBase clientPoint = (wrapObject.ValueT as ACComponent).GetPointNet(wrapObject.ClientPointName);
                                if (clientPoint != null)
                                    clientPoint.CopyDataOfWrapObject(wrapObject);
                            }
                        }
                    }
                }
            }
        }

        public override void CopyDataOfWrapObject(object cloneOrOriginal)
        {
            if ((_LocalStorage == null) || (cloneOrOriginal == null))
                return;
            W Original = GetWrapObject(cloneOrOriginal);
            if (Original != null)
                Original.CopyDataOfWrapObject((ACPointNetWrapObject<T>)cloneOrOriginal);
        }

#endregion

#region EventHandling Methods
#endregion

    }
}

