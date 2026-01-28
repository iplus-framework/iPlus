// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using gip.core.datamodel;
using System.Threading;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetAsyncRMI'}de{'ACPointNetAsyncRMI'}", Global.ACKinds.TACAbstractClass)]
    public abstract class ACPointNetAsyncRMI<T> : ACPointNetStorableAsyncRMIBase<T, ACPointAsyncRMIWrap<T>>, IACPointAsyncRMI<T>
        where T : ACComponent
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetAsyncRMI()
            : this(null, null, 0)
        {
            _base = new ACPointServiceReal<T, ACPointAsyncRMIWrap<T>>(this);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetAsyncRMI(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
            _base = new ACPointServiceReal<T, ACPointAsyncRMIWrap<T>>(this);
        }
        #endregion

        #region Protected Member
        [IgnoreDataMember]
        protected ACPointServiceReal<T, ACPointAsyncRMIWrap<T>> _base;
        [IgnoreDataMember]
        protected ACPointServiceReal<T, ACPointAsyncRMIWrap<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointServiceReal<T, ACPointAsyncRMIWrap<T>>(this);
                return _base;
            }
        }


        internal readonly ACMonitorObject _20032_LockUnsentAsyncRMI = new ACMonitorObject(20032);

        [IgnoreDataMember]
        protected List<ACPointAsyncRMIWrap<T>> _UnsentAsyncRMI = null;

        [IgnoreDataMember]
        public List<ACPointAsyncRMIWrap<T>> UnsentAsyncRMI
        {
            get
            {
                if (_UnsentAsyncRMI != null)
                    return _UnsentAsyncRMI;

                using (ACMonitor.Lock(_20032_LockUnsentAsyncRMI))
                {
                    if (_UnsentAsyncRMI == null)
                        _UnsentAsyncRMI = new List<ACPointAsyncRMIWrap<T>>();
                }
                return _UnsentAsyncRMI;
            }
        }

        public ACPointAsyncRMIWrap<T> CurrentAsyncRMI
        {
            get
            {

                using (ACMonitor.Lock(LockConnectionList_20040))
                {
                    return ConnectionList.Where(c => c.InProcess == true).FirstOrDefault();
                }
            }
        }
#endregion

#region Override Member
        /// <summary>
        /// Events are not Persistable
        /// </summary>
        /// <returns></returns>
        internal override bool ReStoreFromDB()
        {
            bool result = base.ReStoreFromDB();
            if (!PropertyInfo.IsPersistable)
                return result;
            RestoreUnsetAsyncRMI();
            return result;
        }

        protected virtual void RestoreUnsetAsyncRMI()
        {
            if (!ACRef.IsObjLoaded
                || ACRef.ValueT.ContentTask == null
                || ACRef.ValueT.ACOperationMode == ACOperationModes.Test)
                return;
            ACComponent parentACComponent = ParentACComponent as ACComponent;
            if (parentACComponent == null)
                return;
            ACClassTask acClassTask = parentACComponent.ContentTask;
            if (acClassTask == null)
                return;
            try
            {
                ACClassTaskValue acClassTaskValue = this.ACClassTaskValue;
                if (acClassTaskValue == null)
                {
                    if (ACClassTaskQueue.TaskQueue.MassLoadPropertyValuesOff)
                    {
                        ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                            {
                                acClassTaskValue = acClassTask.ACClassTaskValue_ACClassTask.
                                            Where(c => (c.ACClassPropertyID == ACType.ACTypeID)
                                                    && (c.VBUser != null)
                                                    && (c.VBUserID == parentACComponent.Root.Environment.User.VBUserID))
                                                    .FirstOrDefault();
                            }
                        );
                    }
                    else
                    {
                        acClassTaskValue = ACClassTaskQueue.TaskQueue.GetFromAllPropValues(acClassTask.ACClassTaskID,
                                                                                        ACType.ACTypeID,
                                                                                        parentACComponent.Root.Environment.User.VBUserID);
                    }
                    this.ACClassTaskValue = acClassTaskValue;
                }
                if (acClassTaskValue != null)
                {
                    string xmlValue2 = null;
                    ACClassTaskQueue.TaskQueue.ProcessAction(() => { xmlValue2 = acClassTaskValue.XMLValue2; });
                    if (!String.IsNullOrEmpty(xmlValue2))
                    {

                        using (ACMonitor.Lock(_20032_LockUnsentAsyncRMI))
                        {
                            using (StringReader ms = new StringReader(xmlValue2))
                            using (XmlTextReader xmlReader = new XmlTextReader(ms))
                            {
                                DataContractSerializer serializer = new DataContractSerializer(typeof(List<ACPointAsyncRMIWrap<T>>), new DataContractSerializerSettings() { KnownTypes = ACKnownTypes.GetKnownType(), MaxItemsInObjectGraph = 99999999,  IgnoreExtensionDataObject = true, PreserveObjectReferences = true, DataContractResolver = ACConvert.MyDataContractResolver });
                                _UnsentAsyncRMI = (List<ACPointAsyncRMIWrap<T>>)serializer.ReadObject(xmlReader);
                            }
                            if (_UnsentAsyncRMI != null && _UnsentAsyncRMI.Any())
                            {
                                _UnsentAsyncRMI.ForEach(c => c.Point = this);
                                if ((ACRoot.SRoot.Communications != null) && (ACRoot.SRoot.Communications.WCFServiceManager != null))
                                {
#if !ANDROID
                                    ACRoot.SRoot.Communications.WCFServiceManager.SubscriptionUpdatedEvent += WCFServiceManager_SubscriptionUpdatedEvent;
#endif
                                }
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

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPointNetAsyncRMI<T>", "RestoreUnsetAsyncRMI", msg);
            }
        }

        /// <summary>
        /// Events are not Persistable
        /// </summary>
        /// <returns></returns>
        public override bool Persist(bool withLock)
        {
            bool result = base.Persist(withLock);
            if (!PropertyInfo.IsPersistable)
                return result;
            PersistUnsent(withLock);
            return result;
        }


        protected virtual void PersistUnsent(bool withLock)
        {
            if (!ACRef.IsObjLoaded
                || ACRef.ValueT.ContentTask == null
                || ACRef.ValueT.ACOperationMode == ACOperationModes.Test)
                return;
            ACComponent parentACComponent = ParentACComponent as ACComponent;
            if (parentACComponent == null)
                return;
            ACClassTask contentTask = parentACComponent.ContentTask;
            ACClassWF contentClassWF = parentACComponent.Content as ACClassWF;
            // TODO: WorkOrderWF haben keine Service-Punkte
            if (contentTask == null || contentClassWF != null)
                return;
            try
            {
                string valueXML = "";

                using (ACMonitor.Lock(_20032_LockUnsentAsyncRMI))
                {
                    if (_UnsentAsyncRMI != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        using (StringWriter sw = new StringWriter(sb))
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                        {
                            DataContractSerializer serializer = new DataContractSerializer(typeof(List<ACPointAsyncRMIWrap<T>>), new DataContractSerializerSettings() { KnownTypes = ACKnownTypes.GetKnownType(), MaxItemsInObjectGraph = 99999999, IgnoreExtensionDataObject = true, PreserveObjectReferences = true, DataContractResolver = ACConvert.MyDataContractResolver });
                            serializer.WriteObject(xmlWriter, _UnsentAsyncRMI);
                            valueXML = sw.ToString();
                        }
                    }
                }

                ACClassTaskValue axClassTaskValue = ACClassTaskValueWithInit;
                if (axClassTaskValue != null)
                {
                    ACClassTaskQueue.TaskQueue.Add(() => 
                        {
                            axClassTaskValue.XMLValue2 = valueXML;
                        }
                    );
                }

                //Msg msg = ACRef.Obj.Root.Database.ACSaveChanges();
                //if (msg != null)
                //{
                //    ACRef.Obj.Messages.LogMessage(msg);
                //    ACRef.Obj.Root.Database.ACUndoChanges();
                //}
                //result = true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPointNetAsyncRMI<T>", "PersistUnsent", msg);
            }
        }

        protected override ACPointAsyncRMIWrap<T> OnGetNewWrapInstance(T refObject, ACPointNetEventDelegate asyncCallbackDelegate, ACMethod acMethod)
        {
            return new ACPointAsyncRMIWrap<T>((T)refObject, this, asyncCallbackDelegate, acMethod);
        }

        protected override ACPointAsyncRMIWrap<T> OnGetNewWrapInstance(T refObject, string asyncCallbackDelegateName, ACMethod acMethod)
        {
            return new ACPointAsyncRMIWrap<T>((T)refObject, this, asyncCallbackDelegateName, acMethod);
        }

        protected override ACPointAsyncRMIWrap<T> OnGetNewWrapInstance(T refObject, ACPointAsyncRMISubscrWrap<T> clientEntry, ACPointNetEventDelegate asyncCallbackDelegate, ACMethod acMethod)
        {
            return new ACPointAsyncRMIWrap<T>((T)refObject, this, clientEntry, asyncCallbackDelegate, acMethod);
        }

        protected override ACPointAsyncRMIWrap<T> OnGetNewWrapInstance(T refObject, ACPointAsyncRMISubscrWrap<T> clientEntry, string asyncCallbackDelegateName, ACMethod acMethod)
        {
            return new ACPointAsyncRMIWrap<T>((T)refObject, this, clientEntry, asyncCallbackDelegateName, acMethod);
        }

        protected override void OnLocalStorageListChanged()
        {
            // Broadcast zu den Clients
            Base.OnLocalStorageListChanged();
        }

        public override void RebuildAfterDeserialization(object parentSubscrObject)
        {
        }

        /// <summary>
        /// List of ACPointAsyncRMIWrap-relations to other Subscription-Points (IACPointAsyncRMISubscr)
        /// </summary>
        [IgnoreDataMember]
        public override IEnumerable<ACPointAsyncRMIWrap<T>> ConnectionList
        {
            get
            {
                return Base.ConnectionList;
            }
            set
            {
                //Base.ConnectionList = value;
            }
        }

        [IgnoreDataMember]
        public new IEnumerable<T> RefObjectList
        {
            get
            {
                return Base.RefObjectList;
            }
        }

        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint"></param>
        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            Base.OnPointReceivedRemote(receivedPoint);
        }

#endregion

#region IACPointAsyncRMI<T> Member

        public ACPointAsyncRMIWrap<T> InvokeAsyncMethod(ACPointNetEventDelegate AsyncCallbackDelegate, ACMethod acMethod)
        {
            return base.InvokeAsyncMethod(null, AsyncCallbackDelegate, acMethod);
        }

        public ACPointAsyncRMIWrap<T> InvokeAsyncMethod(IACComponent fromACComponent, string asyncCallbackDelegateName, ACMethod acMethod)
        {
            return base.InvokeAsyncMethod(null, fromACComponent, asyncCallbackDelegateName, acMethod);
        }

        public new ACPointAsyncRMIWrap<T> InvokeAsyncMethod(ACPointAsyncRMISubscrWrap<T> clientEntry, ACPointNetEventDelegate AsyncCallbackDelegate, ACMethod acMethod)
        {
            if ((acMethod == null) || (clientEntry == null))
                return null;
            acMethod.CopyFromIfDifferent(clientEntry.ACMethodDescriptor);
            return base.InvokeAsyncMethod(clientEntry, AsyncCallbackDelegate, acMethod);
        }

        public ACPointAsyncRMIWrap<T> InvokeAsyncMethod(ACPointAsyncRMISubscrWrap<T> clientEntry, string asyncCallbackDelegateName, ACMethod acMethod)
        {
            if ((acMethod == null) || (clientEntry == null))
                return null;
            acMethod.CopyFromIfDifferent(clientEntry.ACMethodDescriptor);
            return base.InvokeAsyncMethod(clientEntry, clientEntry.ParentACObject as IACComponent, asyncCallbackDelegateName, acMethod);
        }

        public bool InvokeCallbackDelegate(ACMethodEventArgs Result)
        {
            if (CurrentAsyncRMI == null)
                return false;
            return InvokeCallbackDelegate(CurrentAsyncRMI, Result);
        }

        public bool InvokeCallbackDelegate(ACPointAsyncRMIWrap<T> serviceEntry, ACMethodEventArgs Result, PointProcessingState state = PointProcessingState.Deleted)
        {
            ACPointAsyncRMIWrap<T> invokerOfAsyncOrder = null;
            invokerOfAsyncOrder = GetWrapObject(serviceEntry);
            // invokerOfAsyncOrder is null when serviceEntry is Rejected and remove from LocalStorage-List or if remote Invocation
            // This If-Clause determines if this was really an remote invocation

            if (invokerOfAsyncOrder == null
                    && (serviceEntry.AsyncCallbackDelegate != null
                        || (   //(state == PointProcessingState.Rejected || state == PointProcessingState.Deleted) && 
                                 serviceEntry.ValueT is ACComponent
                             && !(serviceEntry.ValueT is ACComponentProxy)))
                )
            {
                invokerOfAsyncOrder = serviceEntry;
            }
            
            // Falls Aufrufer lokales ACObjekt war
            if (invokerOfAsyncOrder != null)    
            {
                if (invokerOfAsyncOrder.State != state)
                    invokerOfAsyncOrder.State = state;
                ACComponent refACObject = null;
                refACObject = (ACComponent)(IACComponent)invokerOfAsyncOrder.ValueT;
                // Invoke locally
                if (invokerOfAsyncOrder.AsyncCallbackDelegate != null)
                {
                    invokerOfAsyncOrder.AsyncCallbackDelegate(this, Result, invokerOfAsyncOrder);
                }
                else if (!String.IsNullOrEmpty(invokerOfAsyncOrder.AsyncCallbackDelegateName))
                {
                    if (refACObject != null)
                        refACObject.ExecuteMethod(invokerOfAsyncOrder.AsyncCallbackDelegateName, new object[] { (IACPointNetBase)this, Result, (IACObject)invokerOfAsyncOrder });
                }
                // Remove Entries of RMI
                if (   (invokerOfAsyncOrder.State == PointProcessingState.Deleted || invokerOfAsyncOrder.State == PointProcessingState.Rejected)
                    && !String.IsNullOrEmpty(invokerOfAsyncOrder.ClientPointName))
                {
                    if (refACObject != null)
                    {
                        IACPointBase clientPoint = refACObject.GetPoint(invokerOfAsyncOrder.ClientPointName);
                        if ((clientPoint != null) && (clientPoint is IACPointAsyncRMISubscr<T>))
                        {
                            IACPointAsyncRMISubscr<T> clientSubscrRMIPoint = clientPoint as IACPointAsyncRMISubscr<T>;
                            clientSubscrRMIPoint.Remove(new ACPointAsyncRMISubscrWrap<T>((T)this.ACRef.ValueT, clientSubscrRMIPoint, this, invokerOfAsyncOrder.MethodACIdentifier, invokerOfAsyncOrder.RequestID));
                        }
                    }
                    Remove(invokerOfAsyncOrder);
                }
                return true;
            }
            // Sonst Aufruf durch Remote-Objekt
            else
            {
                // Invoke Remote
                bool invokerConnected = InvokeRemoteCallbackDelegate(serviceEntry, Result, state);
                if (invokerConnected)
                    return true;
                // Warte bis Client wieder Verbindung hat
                else if (!serviceEntry.AutoRemove)
                {
                    if ((ACRoot.SRoot.Communications != null) && (ACRoot.SRoot.Communications.WCFServiceManager != null))
                    {
                        serviceEntry.ACMethod.ResultValueList = Result;
                        serviceEntry.State = state;
                        serviceEntry.CallbackIsPending = true;

                        using (ACMonitor.Lock(_20032_LockUnsentAsyncRMI))
                        {
                            UnsentAsyncRMI.Add(serviceEntry);
                        }
#if !ANDROID
                        ACRoot.SRoot.Communications.WCFServiceManager.SubscriptionUpdatedEvent += WCFServiceManager_SubscriptionUpdatedEvent;
#endif
                        Persist(true);
                    }
                }
                // Sonst Aufrufer, der kein Service-Objekt ist -> Kein Warten auf Verbindung, beende aktuellen Auftrag automatisch
                else
                {
                    //_CurrentAsyncRMI = null;
                    return true;
                }
            }
            return false;
        }

#if !ANDROID
        private void WCFServiceManager_SubscriptionUpdatedEvent(object sender, EventArgs e)
        {
            if (_UnsentAsyncRMI == null)
            {
                if ((ACRoot.SRoot.Communications != null) && (ACRoot.SRoot.Communications.WCFServiceManager != null))
                    ACRoot.SRoot.Communications.WCFServiceManager.SubscriptionUpdatedEvent -= WCFServiceManager_SubscriptionUpdatedEvent;
                return;
            }

            bool unsentChanged = false;
            List<ACPointAsyncRMIWrap<T>> copiedList = null;

            using (ACMonitor.Lock(_20032_LockUnsentAsyncRMI))
            {
                copiedList = UnsentAsyncRMI.ToList();
            }
            // Versuche nochmals, Auftrag zurückzusenden
            foreach (ACPointAsyncRMIWrap<T> asyncRMI in copiedList)
            {
                bool invokerConnected = InvokeRemoteCallbackDelegate(asyncRMI, asyncRMI.Result, asyncRMI.State);
                if (!invokerConnected)
                    continue;
                if ((ACRoot.SRoot.Communications != null) && (ACRoot.SRoot.Communications.WCFServiceManager != null))
                {
                    if (asyncRMI != null)
                        asyncRMI.CallbackIsPending = false;

                    using (ACMonitor.Lock(_20032_LockUnsentAsyncRMI))
                    {
                        UnsentAsyncRMI.Remove(asyncRMI);
                    }
                    unsentChanged = true;
                }
            }


            using (ACMonitor.Lock(_20032_LockUnsentAsyncRMI))
            {
                if (UnsentAsyncRMI.Count <= 0)
                    _UnsentAsyncRMI = null;
                if (_UnsentAsyncRMI == null)
                {
                    if ((ACRoot.SRoot.Communications != null) && (ACRoot.SRoot.Communications.WCFServiceManager != null))
                        ACRoot.SRoot.Communications.WCFServiceManager.SubscriptionUpdatedEvent -= WCFServiceManager_SubscriptionUpdatedEvent;
                }
            }
            if (unsentChanged)
                Persist(true);
        }
#endif

        protected bool InvokeRemoteCallbackDelegate(ACPointAsyncRMIWrap<T> serviceEntry, ACMethodEventArgs Result, PointProcessingState state = PointProcessingState.Deleted)
        {
            ACPointAsyncRMIWrap<T> invokerOfAsyncOrder = null;
            ACComponent parentACComponent = ParentACComponent as ACComponent;
            if (parentACComponent != null)
            {
                List<ACPSubscrObjService> SubscribedProxyObjects = parentACComponent.SubscribedProxyObjects;
                if (SubscribedProxyObjects != null)
                {
                    foreach (ACPSubscrObjService proxyObj in SubscribedProxyObjects)
                    {
                        ACPointNetAsyncRMIProxy<T> cpProxy = (ACPointNetAsyncRMIProxy<T>)proxyObj.GetConnectionPoint(this.ACIdentifier);
                        if (cpProxy != null)
                        {
                            invokerOfAsyncOrder = cpProxy.GetWrapObject(serviceEntry);
                            // Falls von diesem Client beauftragt
                            if (invokerOfAsyncOrder != null)
                            {
                                if (invokerOfAsyncOrder.State != state)
                                    invokerOfAsyncOrder.State = state;
                                invokerOfAsyncOrder.ACMethod.ResultValueList = Result;
                                if (invokerOfAsyncOrder.InProcess != serviceEntry.InProcess)
                                    invokerOfAsyncOrder.InProcess = serviceEntry.InProcess;
                                cpProxy.PointChangedForBroadcast = true;
                                Base.MarkACObjectOnChangedPoint();
                                Persist(false);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #region IACPointRefNetFrom<T,ACPointAsyncRMIWrap<T>> Member

        /// <summary>
        /// List of ACPointAsyncRMIWrap-relations to other Subscription-Points (IACPointAsyncRMISubscr)
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<ACPointAsyncRMIWrap<T>> ConnectionListLocal
        {
            get
            {
                return Base.ConnectionListLocal;
            }
        }

        [IgnoreDataMember]
        public IEnumerable<T> RefObjectListLocal
        {
            get
            {
                return Base.RefObjectListLocal;
            }
        }

        /// <summary>
        /// Reference to Extension-Class, which implements this interface (workaround for multiple inheritance)
        /// </summary>
        [IgnoreDataMember]
        public ACPointServiceBase<T, ACPointAsyncRMIWrap<T>> ServicePointHelper
        {
            get
            {
                return Base;
            }
        }

        public void BroadcastIfRMIActivationFailed(IACPointNetBase point)
        {
            if (point == this)
            {

                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    RemoveAllRejected(false);
                    CopyDataOfLocalStorageToClientPoints(false);
                }
            }
        }

        public void InvokeSetMethod(IACPointNetBase point)
        {
            bool handled = false;
            if (this.SetMethod != null)
                handled = this.SetMethod(point);
            if (!handled)
                DeQueueInvocationList();
        }

        public void DeQueueInvocationList()
        {
            IEnumerable<ACPointAsyncRMIWrap<T>> invocationList;
            using (ACMonitor.Lock(LockConnectionList_20040))
            {
                invocationList = ConnectionList.ToArray();
            }
            if (!invocationList.Any())
                return;

            // Falls ein Auftrag in Bearbeitung
            if ((CurrentAsyncRMI != null) || (invocationList.Where(c => c.InProcess == true || c.State == PointProcessingState.Deleted).Any()))
                return;
            ACPointAsyncRMIWrap<T> nextRMI = invocationList.Where(c => c.State == PointProcessingState.Accepted).OrderBy(c => c.SequenceNo).FirstOrDefault();
            // Falls in SetMethod, die Liste nicht manipuliert worden ist und kein spezielles Element selektiert worden ist durch Accepted-state
            // Dann Starte Auftrag, der NewEntry ist.
            if (nextRMI == null)
                nextRMI = invocationList.Where(c => c.State == PointProcessingState.NewEntry).OrderBy(c => c.SequenceNo).FirstOrDefault();
            if (nextRMI == null)
                return;
            bool result = ActivateAsyncRMI(nextRMI, true);
            if (!result)
                nextRMI.State = PointProcessingState.Rejected;
        }

        public void ActivateAllNewInvocations()
        {
            IEnumerable<ACPointAsyncRMIWrap<T>> invocationList;
            using (ACMonitor.Lock(LockConnectionList_20040))
            {
                invocationList = ConnectionList.ToArray();
            }
            if (!invocationList.Any())
                return;

            var query = invocationList 
                        .Where(c => c.State == PointProcessingState.NewEntry && !c.InProcess) 
                        .OrderBy(c => c.SequenceNo);
            if (!query.Any())
                return;
            foreach (ACPointAsyncRMIWrap<T> nextRMI in query)
            {
                if (nextRMI != null)
                {
                    bool result = ActivateAsyncRMI(nextRMI, true);
                    if (!result)
                    {
                        nextRMI.State = PointProcessingState.Rejected;
                    }
                }
            }
        }

        public bool ActivateAsyncRMI(ACPointAsyncRMIWrap<T> wrapObject, bool executeMethod, IACComponent executingInstance = null)
        {
            if (wrapObject == null)
                return false;

            ACMethodEventArgs eventArgs = null;
            bool result = false;
            if (ACRef.ValueT != null)
            {
                result = true;
                wrapObject.InProcess = true;
                object objResult = ((ACComponent)ACRef.ValueT).ExecuteMethod(AsyncMethodInvocationMode.Asynchronous, wrapObject.MethodACIdentifier, wrapObject.ACMethod);
                // Falls keine void-Methode
                if (objResult != null)
                {
                    if (objResult is ACMethodEventArgs)
                    {
                        eventArgs = objResult as ACMethodEventArgs;
                    }
                    else if (objResult is IConvertible)
                    {
                        result = System.Convert.ToBoolean(objResult);
                        if ((executingInstance == null) && result)
                        {
                            executingInstance = ACRef.ValueT.FindChildComponents<IACComponentProcessFunction>(c => c is IACComponentProcessFunction
                                        && (c as IACComponentProcessFunction).CurrentACMethod != null
                                        && (c as IACComponentProcessFunction).CurrentACMethod.ValueT != null
                                        && (c as IACComponentProcessFunction).CurrentACMethod.ValueT.ACRequestID == wrapObject.ACMethod.ACRequestID)
                                .FirstOrDefault();
                        }
                        // Aufruf nicht erfolgreich
                        else if (!result)
                        {
                        }
                    }
                }
                else
                {
                    result = true;
                }

                // Falls asynchrone Methode, synchron durchlaufen, dann ist wurde im wrapObject zuvor der InProcess-Wert oder State gesetzt
                bool wasExecutedSynchronous = !wrapObject.InProcess || (wrapObject.State == PointProcessingState.Deleted);

                //wrapObject.State = PABaseState.SMRunning; // ACPointNetWrapObject<T>.ProcessingState.InProcess;
                if (!wasExecutedSynchronous &&
                    (   ((wrapObject.ExecutingInstance != null) && (executingInstance == null))
                     || ((wrapObject.ExecutingInstance == null) && (executingInstance != null))))
                {
                    wrapObject.SetExecutingInstance(executingInstance);
                }

                // Falls Rückgabewert der Methode ACMethodEventArgs, dann ist evtl. die aufgerufene Methode eine synchrone Methode
                if (eventArgs != null)
                {
                    if (eventArgs.ResultState == Global.ACMethodResultState.Failed 
                        || eventArgs.ResultState == Global.ACMethodResultState.Notpossible 
                        || eventArgs.ResultState == Global.ACMethodResultState.FailedAndRepeat)
                        wrapObject.State = PointProcessingState.Rejected;
                    else if (eventArgs.ResultState == Global.ACMethodResultState.InProcess && wrapObject.State < PointProcessingState.Accepted)
                        wrapObject.State = PointProcessingState.Accepted;
                    else if (eventArgs.ResultState == Global.ACMethodResultState.Succeeded)
                        wrapObject.State = PointProcessingState.Deleted;
                }
                else
                {
                    if (!result)
                    {
                        wrapObject.State = PointProcessingState.Rejected;
                        eventArgs = new ACMethodEventArgs(wrapObject.ACMethod, Global.ACMethodResultState.Failed);
                    }
                    else
                    {
                        wrapObject.State = PointProcessingState.Accepted;
                        eventArgs = new ACMethodEventArgs(wrapObject.ACMethod, Global.ACMethodResultState.InProcess);
                    }
                }

                // Falls Wrap-Object nicht sofort erledigt wurde, weil nicht synchron ausgeführt
                if (result && !wasExecutedSynchronous)
                    Persist(false);
            }
            else
            {
                wrapObject.State = PointProcessingState.Rejected;
                eventArgs = new ACMethodEventArgs(wrapObject.ACMethod, Global.ACMethodResultState.Failed);                
            }


            ACPointAsyncRMIWrap<T> invokerOfAsyncOrder = null;
            invokerOfAsyncOrder = GetWrapObject(wrapObject);
            // Falls Aufrufer lokales ACObjekt war
            if (invokerOfAsyncOrder != null)
            {
                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    RemoveAllRejected(false);
                }
                InvokeCallbackDelegate(wrapObject, eventArgs, wrapObject.State);
            }
            else
            {
                wrapObject.Result = eventArgs;
                BroadcastIfRMIActivationFailed(this);
            }

            return result;
        }

#endregion

    }
}

