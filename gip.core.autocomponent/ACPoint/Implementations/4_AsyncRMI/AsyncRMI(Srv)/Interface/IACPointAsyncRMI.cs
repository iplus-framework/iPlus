using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACPointAsyncRMI<T> : IACPointNet<T, ACPointAsyncRMIWrap<T>>, IACPointNetService<T, ACPointAsyncRMIWrap<T>>
        where T : ACComponent 
    {
        #region Invoke without Client-Point-entry
        /// <summary>
        /// /// Invokes an AsyncMethod an registers a Callback-Method, which is defined in an Assembly-Class
        /// Adds a "refObject" to the List by creating a ACPointRefNetAsyncRMI-Instance and wrapping the "refObject".
        /// AsyncCallbackDelegate-Member, AsyncMethod-Member of ACPointRefNetAsyncRMI will automatically be set.
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <param name="AsyncMethod"></param>
        /// <param name="parameter"></param>
        /// <param name="AutoRemove">
        /// If Async-Call is Processed/Finished on Server-Side then Callback to Client-Object will be made.
        /// If Client-Object is not reachable any more or it is stopped:
        ///     If AutoRemove is not set, ProcessingState remains Completed until Client-Object is reachable again
        ///     -> Server-Object waits with processing of the next Async-Call in it's Point-List
        ///     This Mode is in case of, if Client-Point is also Storable e.g. Model or Workflow-Objects
        ///     If AutoRemove is set, and Client-Object is not reachable. Then Entry in Point will be automatically Removed.
        ///     This Mode is in case of, if Client-Point is not Storable and is a dynamic Instance e.g. BSO-Objects.
        /// </param>
        ACPointAsyncRMIWrap<T> InvokeAsyncMethod(ACPointNetEventDelegate AsyncCallbackDelegate, ACMethod acMethod);

        /// <summary>
        /// Invokes an AsyncMethod an registers a Callback-Method, which is defined by script
        /// Once a AsyncMethod is invoked, a cancellation is not possible any more. So caller must wait until Callback is executed.
        /// </summary>
        /// <param name="fromObject"></param>
        /// <param name="asyncCallbackDelegateName"></param>
        /// <param name="AsyncMethod"></param>
        /// <param name="parameter"></param>
        /// <param name="AutoRemove">
        /// If Async-Call is Processed/Finished on Server-Side then Callback to Client-Object will be made.
        /// If Client-Object is not reachable any more or it is stopped:
        ///     If AutoRemove is not set, ProcessingState remains Completed until Client-Object is reachable again
        ///     -> Server-Object waits with processing of the next Async-Call in it's Point-List
        ///     This Mode is in case of, if Client-Point is also Storable e.g. Model or Workflow-Objects
        ///     If AutoRemove is set, and Client-Object is not reachable. Then Entry in Point will be automatically Removed.
        ///     This Mode is in case of, if Client-Point is not Storable and is a dynamic Instance e.g. BSO-Objects.
        /// </param>
        /// <returns></returns>
        ACPointAsyncRMIWrap<T> InvokeAsyncMethod(IACComponent fromACComponent, string asyncCallbackDelegateName, ACMethod acMethod);
        #endregion

        #region Invoke with Client-Point-entry
        /// <summary>
        /// Invokes an AsyncMethod an registers a Callback-Method, which is defined in an Assembly-Class
        /// Adds a "refObject" to the List by creating a ACPointRefNetAsyncRMI-Instance and wrapping the "refObject".
        /// AsyncCallbackDelegate-Member, AsyncMethod-Member of ACPointRefNetAsyncRMI will automatically be set.
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="clientEntry"></param>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <param name="parameter"></param>
        /// <param name="AutoRemove"></param>
        /// <returns></returns>
        ACPointAsyncRMIWrap<T> InvokeAsyncMethod(ACPointAsyncRMISubscrWrap<T> clientEntry, ACPointNetEventDelegate AsyncCallbackDelegate, ACMethod acMethod);

        /// <summary>
        /// Invokes an AsyncMethod an registers a Callback-Method, which is defined by script
        /// Once a AsyncMethod is invoked, a cancellation is not possible any more. So caller must wait until Callback is executed.
        /// </summary>
        /// <param name="clientEntry"></param>
        /// <param name="asyncCallbackDelegateName"></param>
        /// <param name="parameter"></param>
        /// <param name="AutoRemove"></param>
        /// <returns></returns>
        ACPointAsyncRMIWrap<T> InvokeAsyncMethod(ACPointAsyncRMISubscrWrap<T> clientEntry, string asyncCallbackDelegateName, ACMethod acMethod);
        #endregion

        #region InvokeCallbackDelegate

        /// <summary>
        /// Invoke's Callback-Method for current RMI-Entry
        /// Signals that Method was completed
        /// </summary>
        /// <param name="Result"></param>
        /// <returns></returns>
        bool InvokeCallbackDelegate(ACMethodEventArgs Result);

        /// <summary>
        /// Invoke Callback-Method for selectable RMI-Entry
        /// </summary>
        /// <param name="serviceEntry"></param>
        /// <param name="Result"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        bool InvokeCallbackDelegate(ACPointAsyncRMIWrap<T> serviceEntry, ACMethodEventArgs Result, PointProcessingState state = PointProcessingState.Deleted);
        #endregion
    }

    public interface IACPointAsyncRMI : IACPointAsyncRMI<ACComponent>
    {
        /// <summary>
        /// Add a Task at Target-Component only if ACIdentifier of this IACPointAsyncRMI is "TaskInvocationPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMI<T>-Interface
        /// </summary>
        /// <param name="acMethod"></param>
        /// <returns></returns>
        IACPointEntry AddTask(ACMethod acMethod, IACComponentTaskSubscr subscriber);

        /// <summary>
        /// Remove a Task at Target-Component only if ACIdentifier of this IACPointAsyncRMI is "TaskInvocationPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMI<T>-Interface
        /// </summary>
        /// <param name="acJob"></param>
        /// <returns></returns>
        bool RemoveTask(ACMethod acMethod, IACComponentTaskSubscr subscriber);

        void ClearMyInvocations(IACComponentTaskSubscr subscriber);
    }
}

