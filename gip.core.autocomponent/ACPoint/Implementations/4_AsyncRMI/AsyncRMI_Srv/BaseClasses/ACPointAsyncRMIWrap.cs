// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    public class ACPointAsyncRMIWrap<T> : ACPointEventWrap<T>, IACTask
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>Constructor for contructing the Wrapper-"wrapObject"</summary>
        /// <param name="refObject">A "refObject" muss be passed. It will be wrapped from this "wrapObject"</param>
        /// <param name="owner"></param>
        /// <param name="asyncCallbackDelegate"></param>
        /// <param name="acMethod"></param>
        public ACPointAsyncRMIWrap(T refObject, IACPointNetBase owner, ACPointNetEventDelegate asyncCallbackDelegate,
                                    ACMethod acMethod)
            : base(refObject, owner, asyncCallbackDelegate)
        {
            _ACMethod = acMethod;
        }



        /// <summary>Initializes a new instance of the <see cref="ACPointAsyncRMIWrap{T}" /> class.</summary>
        /// <param name="refObject">The reference object.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="clientEntry">The client entry.</param>
        /// <param name="asyncCallbackDelegate">The asynchronous callback delegate.</param>
        /// <param name="acMethod">The ac method.</param>
        public ACPointAsyncRMIWrap(T refObject, IACPointNetBase owner, ACPointAsyncRMISubscrWrap<T> clientEntry,
                                    ACPointNetEventDelegate asyncCallbackDelegate, ACMethod acMethod)
            : base(refObject, owner, asyncCallbackDelegate)
        {
            acMethod.CopyFromIfDifferent(clientEntry.ACMethodDescriptor);
            _ACMethod = acMethod;
            _ClientPointName = clientEntry.Point.ACIdentifier;
        }

        /// <summary>Constructor for contructing the Wrapper-"wrapObject", if Callback-Method is defined by script</summary>
        /// <param name="refObject">A "refObject" muss be passed. It will be wrapped from this "wrapObject"</param>
        /// <param name="owner"></param>
        /// <param name="asyncCallbackDelegateName"></param>
        /// <param name="acMethod"></param>
        public ACPointAsyncRMIWrap(T refObject, IACPointNetBase owner, string asyncCallbackDelegateName, ACMethod acMethod)
            : base(refObject, owner, asyncCallbackDelegateName)
        {
            _ACMethod = acMethod;
        }

        /// <summary>Initializes a new instance of the <see cref="ACPointAsyncRMIWrap{T}" /> class.</summary>
        /// <param name="refObject">The reference object.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="clientEntry">The client entry.</param>
        /// <param name="asyncCallbackDelegateName">Name of the asynchronous callback delegate.</param>
        /// <param name="acMethod">The ac method.</param>
        public ACPointAsyncRMIWrap(T refObject, IACPointNetBase owner, ACPointAsyncRMISubscrWrap<T> clientEntry, string asyncCallbackDelegateName, ACMethod acMethod)
            : base(refObject, owner, asyncCallbackDelegateName)
        {
            acMethod.CopyFromIfDifferent(clientEntry.ACMethodDescriptor);
            _ACMethod = acMethod;
            _ClientPointName = clientEntry.Point.ACIdentifier;
        }

        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointAsyncRMIWrap()
        {
        }
        #endregion

        #region IACTask
        [IgnoreDataMember]
        public override Guid RequestID
        {
            get
            {
                if (_ACMethod == null)
                    return Guid.Empty;
                return _ACMethod.ACRequestID;
            }

            internal set
            {
            }
        }

        IACWorkflowContext _WorkflowContext;
        public IACWorkflowContext WorkflowContext
        {
            get
            {

                using (ACMonitor.Lock(this.LockRef_10030))
                {
                    if (_WorkflowContext != null)
                        return _WorkflowContext;
                }

                IACWorkflowContext workflowContext = null;
                if (ACMethod != null)
                {
                    var acValuePID = ACMethod.ParameterValueList.GetACValue(ACProgram.ClassName);
                    if (acValuePID != null)
                    {
                        Guid acProgramID = (Guid)acValuePID.Value;
                        if (acProgramID != Guid.Empty)
                        {
                            workflowContext = ACClassTaskQueue.TaskQueue.ProgramCache.GetProgram(acProgramID);
                        }
                    }
                }

                using (ACMonitor.Lock(this.LockRef_10030))
                {
                    if (_WorkflowContext == null && workflowContext != null)
                        _WorkflowContext = workflowContext;
                    return _WorkflowContext;
                }
            }
        }

        /// <summary>
        /// If Async-Call is Processed/Finished on Server-Side then Callback to Client-Object will be made.
        /// If Client-Object is not reachable any more or it is stopped:
        ///     If AutoRemove is not set, ProcessingState remains Completed until Client-Object is reachable again
        ///     -> Server-Object waits with processing of the next Async-Call in it's Point-List
        ///     This Mode is in case of, if Client-Point is also Storable e.g. Model or Workflow-Objects
        ///     If AutoRemove is set, and Client-Object is not reachable. Then Entry in Point will be automatically Removed.
        ///     This Mode is in case of, if Client-Point is not Storable and is a dynamic Instance e.g. BSO-Objects.
        /// </summary>
        [IgnoreDataMember]
        public bool AutoRemove
        {
            get
            {
                if (_ACMethod == null)
                    return false;
                return _ACMethod.AutoRemove;
            }
        }

        [IgnoreDataMember]
        private ACMethod _ACMethod;
        [DataMember]
        public ACMethod ACMethod
        {
            get
            {
                return _ACMethod;
            }
            set
            {
                _ACMethod = value;
            }
        }

        [IgnoreDataMember]
        public ACValueList Parameter
        {
            get
            {
                if (_ACMethod == null)
                    return null;
                return _ACMethod.ParameterValueList;
            }
        }

        [IgnoreDataMember]
        private bool _CallbackIsPending = false;
        [DataMember]
        public bool CallbackIsPending
        {
            get
            {
                return _CallbackIsPending;
            }
            internal set
            {
                _CallbackIsPending = value;
            }
        }

        [DataMember]
        public Boolean InProcess
        {
            get;
            set;
        }

        [DataMember]
        public ACRef<IACComponent> ExecutingInstance
        {
            get;
            set;
        }

        /// <summary>
        /// Name of Method, which will be called on the Server-Object
        /// </summary>
        [IgnoreDataMember]
        public string MethodACIdentifier
        {
            get
            {
                if (_ACMethod == null)
                    return "";
                return _ACMethod.ACIdentifier;
            }
        }


        [IgnoreDataMember]
        public ACMethodEventArgs Result
        {
            get
            {
                if (_ACMethod == null)
                    return null;
                if (_ACMethod.ResultValueList is ACMethodEventArgs)
                    return _ACMethod.ResultValueList as ACMethodEventArgs;
                return new ACMethodEventArgs(_ACMethod, Global.ACMethodResultState.InProcess);
            }
            set
            {
                if (_ACMethod == null)
                    return;
                _ACMethod.ResultValueList = value;
            }
        }


        public void SetExecutingInstance(IACComponent serverSideInstance)
        {
            if (serverSideInstance == null)
                return;
            if (serverSideInstance.IsProxy)
                return;
            ExecutingInstance = new ACRef<IACComponent>(serverSideInstance, ACRef<IACComponent>.RefInitMode.AutoStart, true, ParentACObject);
        }


        protected override void OnPointReceivedRemote()
        {
            if (_ACMethod == null)
                return;
            if (_ACMethod.ParameterValueList != null)
                _ACMethod.ParameterValueList.AttachTo(ParentACObject);
            if (_ACMethod.ResultValueList != null)
                _ACMethod.ResultValueList.AttachTo(ParentACObject);
            if (ExecutingInstance != null)
                ExecutingInstance.AttachTo(ParentACObject);

            //if (Parameter != null)
            //{
            //    foreach (object param in Parameter)
            //    {
            //        if (param is IACRef)
            //            (param as IACRef).AttachTo(ParentACObject);
            //        else if (param is ACEventArgs)
            //            (param as ACEventArgs).AttachTo(ParentACObject as IACComponent);
            //    }
            //}
            //if (Result != null)
            //{
            //    Result.AttachTo(ParentACObject as IACComponent);
            //}
        }



#endregion

#region IComparable Member

        public override int CompareToAtRemove(object obj)
        {
            int diff = base.CompareToAtRemove(obj);
            if (diff == 0)
            {
                if (ExecutingInstance != null)
                    ExecutingInstance.Detach();
            }
            return diff;
        }

        public override int CompareTo(object obj)
        {
            if (obj is ACPointAsyncRMIWrap<T>)
            {
                ACPointAsyncRMIWrap<T> objRMI = obj as ACPointAsyncRMIWrap<T>;
                int diff = this.GetACUrl().CompareTo(objRMI.GetACUrl());
                if (diff != 0)
                    return diff;
                diff = this.MethodACIdentifier.CompareTo(objRMI.MethodACIdentifier);
                if (diff != 0)
                    return diff;
                return this.RequestID.CompareTo(objRMI.RequestID);
            }
            else if (obj is ACPointAsyncRMISubscrWrap<T>)
            {
                ACPointAsyncRMISubscrWrap<T> objRMI = obj as ACPointAsyncRMISubscrWrap<T>;
                int diff = 0;
                if (this.ParentACObject != null)
                {
                    diff = (this.ParentACObject as IACObject).GetACUrl().CompareTo(objRMI.GetACUrl());
                    if (diff != 0)
                        return diff;
                }
                diff = this.MethodACIdentifier.CompareTo(objRMI.MethodACIdentifier);
                if (diff != 0)
                    return diff;
                return this.RequestID.CompareTo(objRMI.RequestID);
            }
            else
                return -1;
        }

        public override void CompareChangedData(ACPointNetWrapObject<T> objUpdate)
        {
            base.CompareChangedData(objUpdate);
            if (!this.DataHasChanged)
            {
                if (objUpdate is ACPointAsyncRMIWrap<T>)
                {
                    ACPointAsyncRMIWrap<T> objRMI = objUpdate as ACPointAsyncRMIWrap<T>;
                    if (((objRMI.ExecutingInstance != null) && (this.ExecutingInstance == null))
                        || ((objRMI.ExecutingInstance == null) && (this.ExecutingInstance != null)))
                        this.DataHasChanged = true;
                    else if (objRMI.InProcess != this.InProcess)
                        this.DataHasChanged = true;
                }
            }
        }

        public override void CopyDataOfWrapObject(ACPointNetWrapObject<T> obj)
        {
            base.CopyDataOfWrapObject(obj);
        }

#endregion

    }
}
