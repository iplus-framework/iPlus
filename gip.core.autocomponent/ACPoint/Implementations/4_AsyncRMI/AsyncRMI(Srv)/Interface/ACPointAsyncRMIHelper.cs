using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public static class ACPointAsyncRMIHelper
    {
        #region Helper-Methods for IACPointAsyncRMI
        internal static bool AddTask(IACPointAsyncRMI rmiInvocationPoint, ACMethod acMethod, IACComponentTaskSubscr subscriber)
        {
            if (subscriber == null || acMethod == null || rmiInvocationPoint == null || !acMethod.IsValid())
                return false;
            if (rmiInvocationPoint.ACIdentifier != Const.TaskInvocationPoint)
                return false;
            ACComponent subscriberComp = subscriber as ACComponent;
            if (subscriberComp == null)
                return false;

            IACPointAsyncRMISubscr rmiSubscriptionPoint = subscriber.GetPoint(Const.TaskSubscriptionPoint) as IACPointAsyncRMISubscr;
            if (rmiSubscriptionPoint == null)
                return false;

            IACObject wrapObject = null;
            ACPointNetEventDelegate cbDelegate = subscriber.TaskCallbackDelegate;
            if (cbDelegate != null)
            {
                if (rmiSubscriptionPoint == null)
                    wrapObject = rmiInvocationPoint.InvokeAsyncMethod(cbDelegate, acMethod);
                else
                    wrapObject = rmiSubscriptionPoint.InvokeAsyncMethod(rmiInvocationPoint.ParentACComponent, rmiInvocationPoint.ACIdentifier, acMethod, cbDelegate);
            }
            else
            {
                if (rmiSubscriptionPoint == null)
                    wrapObject = rmiInvocationPoint.InvokeAsyncMethod(subscriber, Const.TaskCallback, acMethod);
                else
                    wrapObject = rmiSubscriptionPoint.InvokeAsyncMethod(rmiInvocationPoint.ParentACComponent, rmiInvocationPoint.ACIdentifier, acMethod, Const.TaskCallback);
            }

            return wrapObject != null;
        }

        internal static bool RemoveTask(IACPointAsyncRMI rmiInvocationPoint, ACMethod acMethod, IACComponentTaskSubscr subscriber)
        {
            if (subscriber == null || acMethod == null || rmiInvocationPoint == null)
                return false;
            if (rmiInvocationPoint.ACIdentifier != Const.TaskInvocationPoint)
                return false;

            ACComponent subscriberComp = subscriber as ACComponent;
            if (subscriberComp == null)
                return false;

            IACPointAsyncRMISubscr rmiSubscriptionPoint = subscriber.GetPoint(Const.TaskSubscriptionPoint) as IACPointAsyncRMISubscr;
            if (rmiSubscriptionPoint == null)
                return false;

            IACObject wrapObject = null;
            ACMethodEventArgs result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.Failed);
            if (!rmiInvocationPoint.InvokeCallbackDelegate(result))
            {
                wrapObject = rmiInvocationPoint.ConnectionList.Where(c => c.RequestID == acMethod.ACRequestID).FirstOrDefault();
                if (wrapObject != null)
                    rmiInvocationPoint.Remove(wrapObject);

                wrapObject = subscriber.TaskSubscriptionPoint.ConnectionList.Where(c => c.RequestID == acMethod.ACRequestID).FirstOrDefault();
                if (wrapObject != null)
                    subscriber.TaskSubscriptionPoint.Remove(wrapObject);
            }

            return true;
        }
        #endregion

        #region Helper-Methods for IACPointAsyncRMISubscr

        internal static bool SubscribeTask(IACPointAsyncRMISubscr rmiSubscriptionPoint, ACMethod acMethod, ACComponent atComponent)
        {
            if (atComponent == null || acMethod == null || rmiSubscriptionPoint == null || !acMethod.IsValid())
                return false;
            if (rmiSubscriptionPoint.ACIdentifier != Const.TaskSubscriptionPoint)
                return false;
            IACComponentTaskSubscr subscriber = rmiSubscriptionPoint.ParentACComponent as IACComponentTaskSubscr;
            if (subscriber == null)
                return false;

            IACPointAsyncRMI rmiInvocationPoint = atComponent.GetPoint(Const.TaskInvocationPoint) as IACPointAsyncRMI;
            if (rmiInvocationPoint == null)
                return false;

            IACObject wrapObject = null;
            ACPointNetEventDelegate cbDelegate = subscriber.TaskCallbackDelegate;
            if (cbDelegate != null)
            {
                if (rmiSubscriptionPoint == null)
                    wrapObject = rmiInvocationPoint.InvokeAsyncMethod(cbDelegate, acMethod);
                else
                    wrapObject = rmiSubscriptionPoint.InvokeAsyncMethod(atComponent, rmiInvocationPoint.ACIdentifier, acMethod, cbDelegate);
            }
            else
            {
                if (rmiSubscriptionPoint == null)
                    wrapObject = rmiInvocationPoint.InvokeAsyncMethod(subscriber, Const.TaskCallback, acMethod);
                else
                    wrapObject = rmiSubscriptionPoint.InvokeAsyncMethod(atComponent, rmiInvocationPoint.ACIdentifier, acMethod, Const.TaskCallback);
            }

            return wrapObject != null;
        }

        internal static bool UnSubscribeTask(IACPointAsyncRMISubscr rmiSubscriptionPoint, ACMethod acMethod, ACComponent atComponent)
        {
            if (rmiSubscriptionPoint.ACIdentifier != Const.TaskSubscriptionPoint)
                return false;

            IACComponentTaskSubscr subscriber = rmiSubscriptionPoint.ParentACComponent as IACComponentTaskSubscr;
            if (subscriber == null)
                return false;

            IACPointAsyncRMI rmiInvocationPoint = atComponent.GetPoint(Const.TaskInvocationPoint) as IACPointAsyncRMI;
            if (rmiInvocationPoint == null)
                return false;

            return rmiInvocationPoint.RemoveTask(acMethod, subscriber);
        }

        internal static bool UnSubscribeAll(IACPointAsyncRMISubscr rmiSubscriptionPoint)
        {
            if (rmiSubscriptionPoint.ACIdentifier != Const.TaskSubscriptionPoint)
                return false;

            IACComponentTaskSubscr subscriber = rmiSubscriptionPoint.ParentACComponent as IACComponentTaskSubscr;
            if (subscriber == null)
                return false;

            ACPointNetStorableAsyncRMISubscrBase<ACComponent, ACPointAsyncRMISubscrWrap<ACComponent>> baseSubscriptionPoint = rmiSubscriptionPoint as ACPointNetStorableAsyncRMISubscrBase<ACComponent, ACPointAsyncRMISubscrWrap<ACComponent>>;
            if (baseSubscriptionPoint == null)
                return false;


            IEnumerable<ACPointAsyncRMISubscrWrap<ACComponent>> query = null;

            using (ACMonitor.Lock(baseSubscriptionPoint.LockLocalStorage_20033))
            {
                query = baseSubscriptionPoint.LocalStorage.ToArray();
            }

            if (query != null && query.Any())
            {
#if DEBUG
                //System.Diagnostics.Debugger.Break();
#endif
                foreach (var wrapObject in query)
                {
                    ACComponent serviceComp = (wrapObject.ValueT as ACComponent);
                    if (serviceComp == null)
                        continue;

                    if (!String.IsNullOrEmpty(wrapObject.RMIPointName))
                    {
                        IACPointAsyncRMI rmiInvocationPoint = serviceComp.GetPointNet(wrapObject.RMIPointName) as IACPointAsyncRMI;
                        if (rmiInvocationPoint != null)
                        {
                            rmiInvocationPoint.Remove(wrapObject);
                            baseSubscriptionPoint.Remove(wrapObject);
                        }
                    }
                    else
                    {
                        IACComponentTaskExec serviceTaskExec = serviceComp as IACComponentTaskExec;
                        if (serviceTaskExec != null)
                        {
                            IACPointAsyncRMI rmiInvocationPoint = serviceTaskExec.TaskInvocationPoint;
                            if (rmiInvocationPoint != null)
                            {
                                rmiInvocationPoint.Remove(wrapObject);
                                baseSubscriptionPoint.Remove(wrapObject);
                            }
                        }
                    }
                }
            }
            return true;
        }

        internal static void ClearMyInvocations(IACPointAsyncRMI rmiInvocationPoint, IACComponentTaskSubscr rmiSubscriptionPoint)
        {
            string subscriberUrl = rmiSubscriptionPoint.GetACUrl();
            ACPointNetStorableAsyncRMIBase<ACComponent, ACPointAsyncRMIWrap<ACComponent>> baseInvocationPoint = rmiInvocationPoint as ACPointNetStorableAsyncRMIBase<ACComponent, ACPointAsyncRMIWrap<ACComponent>>;
            if (baseInvocationPoint == null)
                return;

            IEnumerable<ACPointAsyncRMIWrap<ACComponent>> query = null;

            using (ACMonitor.Lock(baseInvocationPoint.LockLocalStorage_20033))
            {
                query = baseInvocationPoint.LocalStorage.Where(c => c.ACUrl == subscriberUrl).ToArray();
            }

            if (query != null && query.Any())
            {
#if DEBUG
                //System.Diagnostics.Debugger.Break();
#endif
                foreach (var entry in query)
                {
                    baseInvocationPoint.Remove(entry);
                }
            }

            ACPointNetAsyncRMI<ACComponent> serviceRMIPoint = rmiInvocationPoint as ACPointNetAsyncRMI<ACComponent>;
            if (serviceRMIPoint != null)
            {
                using (ACMonitor.Lock(serviceRMIPoint._20032_LockUnsentAsyncRMI))
                {
                    query = serviceRMIPoint.UnsentAsyncRMI.Where(c => c.ACUrl == subscriberUrl).ToArray();
                    if (query != null && query.Any())
                    {
#if DEBUG
                        //System.Diagnostics.Debugger.Break();
#endif
                        foreach (var entry in query)
                        {
                            serviceRMIPoint.UnsentAsyncRMI.Remove(entry);
                        }
                    }
                }
            }
        }
        #endregion

        #region Helper-Methods for IACComponentTaskSubscr
        public static bool ActivateTask(IACComponentTaskExec thisComp, ACMethod acMethod, bool executeMethod, IACComponent executingInstance)
        {
            if (thisComp == null || thisComp.IsProxy)
                return false;
            if (thisComp.TaskInvocationPoint == null)
                return false;
            return thisComp.TaskInvocationPoint.ActivateTask(acMethod, executeMethod, executingInstance);
        }

        public static bool CallbackTask(IACComponentTaskExec thisComp, ACMethod acMethod, ACMethodEventArgs result, PointProcessingState state)
        {
            if (thisComp == null || thisComp.IsProxy)
                return false;
            ACComponent internalComp = thisComp as ACComponent;
            AsyncMethodWaitHandle syncInvocationWaitHandle = internalComp.SyncInvocationWaitHandleList.Where(c => c.InvokedACMethod.ACRequestID == acMethod.ACRequestID).FirstOrDefault();
            if (syncInvocationWaitHandle != null)
            {
                if (syncInvocationWaitHandle.Callback(acMethod, result))
                    return true;
            }

            if (thisComp.TaskInvocationPoint == null)
                return false;
            return thisComp.TaskInvocationPoint.InvokeCallbackDelegate(acMethod, result, state);
        }

        public static bool CallbackTask(IACComponentTaskExec thisComp, IACTask task, ACMethodEventArgs result, PointProcessingState state)
        {
            if (thisComp == null || thisComp.IsProxy)
                return false;
            ACComponent internalComp = thisComp as ACComponent;
            AsyncMethodWaitHandle syncInvocationWaitHandle = internalComp.SyncInvocationWaitHandleList.Where(c => c.InvokedACMethod.ACRequestID == task.ACMethod.ACRequestID).FirstOrDefault();
            if (syncInvocationWaitHandle != null)
            {
                if (syncInvocationWaitHandle.Callback(task.ACMethod, result))
                    return true;
            }

            if (thisComp.TaskInvocationPoint == null)
                return false;
            return thisComp.TaskInvocationPoint.InvokeCallbackDelegate(task, result, state);
        }

        public static bool CallbackCurrentTask(IACComponentTaskExec thisComp, ACMethodEventArgs result)
        {
            if (thisComp == null || thisComp.IsProxy)
                return false;
            if (thisComp.TaskInvocationPoint == null)
                return false;
            return thisComp.TaskInvocationPoint.InvokeCallbackDelegate(result);
        }

        public static IACTask GetTaskOfACMethod(IACComponentTaskExec thisComp, ACMethod acMethod)
        {
            if (thisComp == null)
                return null;
            if (thisComp.TaskInvocationPoint == null)
                return null;
            return thisComp.TaskInvocationPoint.GetTaskOfACMethod(acMethod);
        }

        #endregion
    }
}

