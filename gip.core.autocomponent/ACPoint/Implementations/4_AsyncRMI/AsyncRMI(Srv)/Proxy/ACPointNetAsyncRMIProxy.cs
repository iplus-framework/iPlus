using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetAsyncRMIProxy'}de{'ACPointNetAsyncRMIProxy'}", Global.ACKinds.TACAbstractClass)]
    internal abstract class ACPointNetAsyncRMIProxy<T> : ACPointNetStorableAsyncRMIBase<T, ACPointAsyncRMIWrap<T>>, IACPointAsyncRMI<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetAsyncRMIProxy()
            : this(null, null, 0)
        {
            _base = new ACPointServiceProxy<T, ACPointAsyncRMIWrap<T>>(this, true);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetAsyncRMIProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _base = new ACPointServiceProxy<T, ACPointAsyncRMIWrap<T>>(this, false);
        }
        #endregion

        #region protected Member
        [IgnoreDataMember]
        protected ACPointServiceProxy<T, ACPointAsyncRMIWrap<T>> _base;
        [IgnoreDataMember]
        protected ACPointServiceProxy<T, ACPointAsyncRMIWrap<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointServiceProxy<T, ACPointAsyncRMIWrap<T>>(this, true);
                return _base;
            }
        }
        #endregion

        #region Override Member
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


        public override void Subscribe(bool force = true)
        {
            Base.Subscribe(force);
        }

        public override void ReSubscribe()
        {
            Base.Subscribe(false);
        }

        public override void UnSubscribe()
        {
            Base.UnSubscribe();
        }

        protected override void OnLocalStorageListChanged()
        {
            // Trage im Dispatcher zum Versand ein.
            Base.OnLocalStorageListChanged();
        }

        public override void RebuildAfterDeserialization(object parentSubscrObject)
        {
            Base.RebuildAfterDeserialization(parentSubscrObject);
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
                Base.ConnectionList = value;
            }
        }


        public override string ConnectionListInfo
        {
            get
            {
                Subscribe(false); // Bindings to Tooltips invokes cyclic this Method. To reduce the serverside cpu-usage the subscription-Request should only take one time.
                return base.ConnectionListInfo;
            }
        }

        /// <summary>
        /// Member für Serialisierung, weil ConnectionList nicht serialisiert werden kann,
        /// weil das Interface nur eine get-Anweisung enhält
        /// </summary>
        [DataMember]
        private IEnumerable<ACPointAsyncRMIWrap<T>> SerConnList
        {
            get
            {
                return Base.GetConnectionListForSerialization();
                //return Base.ConnectionList;
            }
            set
            {
                Base.ConnectionList = value;
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
            if (!Base.IsInstanceOnServerSide)
            {
                ACPointNetAsyncRMIProxy<T> receivedProxyPoint = (ACPointNetAsyncRMIProxy<T>)receivedPoint;
                List<ACPointAsyncRMIWrap<T>> executedList = null;
                ACPointAsyncRMIWrap<T>[] callbacks = null;


                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    callbacks = LocalStorage.ToArray();
                }
                if (callbacks != null && callbacks.Any())
                {
                    foreach (ACPointAsyncRMIWrap<T> asyncRMIWrap in callbacks)
                    {
                        //asyncRMIWrap.DataHasChanged
                        if ((asyncRMIWrap.State == PointProcessingState.Deleted
                            || asyncRMIWrap.DataHasChanged)
                            && (asyncRMIWrap.Result != null))
                        {
                            if (executedList == null)
                                executedList = new List<ACPointAsyncRMIWrap<T>>();
                            if (asyncRMIWrap.State == PointProcessingState.Deleted)
                                executedList.Add(asyncRMIWrap);
                            if (!String.IsNullOrEmpty(asyncRMIWrap.AsyncCallbackDelegateName))
                            {
                                ACComponent refACObject = (ACComponent)(IACComponent)asyncRMIWrap.ValueT;
                                if (refACObject != null)
                                {
                                    refACObject.ExecuteMethod(asyncRMIWrap.AsyncCallbackDelegateName, new object[] { (IACPointNetBase)this, (ACEventArgs)asyncRMIWrap.Result, (IACObject)asyncRMIWrap });
                                    // Remove Entries of RMI
                                    if ((asyncRMIWrap.State == PointProcessingState.Deleted) &&
                                        !String.IsNullOrEmpty(asyncRMIWrap.ClientPointName))
                                    {
                                        IACPointBase clientPoint = refACObject.GetPoint(asyncRMIWrap.ClientPointName);
                                        if ((clientPoint != null) && (clientPoint is IACPointAsyncRMISubscr<T>))
                                        {
                                            IACPointAsyncRMISubscr<T> clientSubscrRMIPoint = clientPoint as IACPointAsyncRMISubscr<T>;
                                            clientSubscrRMIPoint.Remove(new ACPointAsyncRMISubscrWrap<T>((T)this.ACRef.ValueT, clientSubscrRMIPoint, this, asyncRMIWrap.MethodACIdentifier, asyncRMIWrap.RequestID));
                                        }
                                    }
                                }
                            }
                            asyncRMIWrap.DataHasChanged = false;
                        }
                    }
                }
                if (executedList != null)
                {
                    foreach (ACPointAsyncRMIWrap<T> executedMethod in executedList)
                    {
                        Remove(executedMethod);
                    }
                }
            }
        }


#endregion

#region IACPointRefNetAsyncRMI<T> Member

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


        /// <summary>
        /// Invoke's Callback-Method for current RMI-Entry
        /// Signals that Method was completed
        /// </summary>
        /// <param name="Result"></param>
        /// <returns></returns>
        public bool InvokeCallbackDelegate(ACMethodEventArgs Result)
        {
            return true;
            //throw new NotImplementedException();
        }

        /// <summary>Invoke Callback-Method for selectable RMI-Entry</summary>
        /// <param name="serviceEntry"></param>
        /// <param name="Result"></param>
        /// <param name="state"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        public bool InvokeCallbackDelegate(ACPointAsyncRMIWrap<T> serviceEntry, ACMethodEventArgs Result, PointProcessingState state = PointProcessingState.Deleted)
        {
            return true;
            //throw new NotImplementedException();
        }

#endregion

#region IACPointRefNetFrom<T,ACPointAsyncRMIWrap<T>> Member

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

        public void InvokeSetMethod(IACPointNetBase point)
        {
            if (this.SetMethod != null)
                this.SetMethod(point);
        }

#endregion
    }
}

