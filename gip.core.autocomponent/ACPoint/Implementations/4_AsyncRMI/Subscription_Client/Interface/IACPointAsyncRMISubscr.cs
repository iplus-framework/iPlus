// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACPointAsyncRMISubscr<T> : IACPointNet<T, ACPointAsyncRMISubscrWrap<T>>, IACPointNetClient
        where T : ACComponent
    {
        /// <summary>
        /// Invokes an AsyncMethod at atACObject. It registers a Callback-Method, which is defined in an Assembly-Class
        /// </summary>
        /// <param name="atACComponent">ACObject which publish an AsyncMethod</param>
        /// <param name="asyncRMIPointName">Name of Point for activating the AsyncMethod</param>
        /// <param name="acMethod">Name of Async-Method, which should be invoked</param>
        /// <param name="AsyncCallbackDelegate">Event-Handler-CallBack-Delegate of this when Asyc-Method is Executed</param>
        /// <returns></returns>
        ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName,
                                                       ACMethod acMethod,
                                                       ACPointNetEventDelegate AsyncCallbackDelegate);

        ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName,
                                                       ACMethod acMethod,
                                                       ACPointNetEventDelegate AsyncCallbackDelegate,
                                                       bool AutoRemove);
        /// <summary>
        /// Invokes an AsyncMethod at atACObject. It registers a Callback-Method, which is defined in an Assembly-Class
        /// </summary>
        /// <param name="atACComponent">ACObject which publish an AsyncMethod</param>
        /// <param name="asyncRMIPointName">Name of Point for activating the AsyncMethod</param>
        /// <param name="acMethod">Name of Async-Method, which should be invoked</param>
        /// <param name="asyncCallbackDelegateName">Event-Handler-CallBack-Delegate of this when Asyc-Method is Executed</param>
        /// <returns></returns>
        ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName,
                                                       ACMethod acMethod,
                                                       string asyncCallbackDelegateName);

        ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName,
                                                       ACMethod acMethod,
                                                       string asyncCallbackDelegateName, bool AutoRemove);
    }

    public interface IACPointAsyncRMISubscr : IACPointAsyncRMISubscr<ACComponent>
    {
        /// <summary>
        /// Subscribes a Task at Source-Component only if ACIdentifier of this IACPointAsyncRMISubscr is "TaskSubscriptionPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMISubscr&lt;T&gt;-Interface
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="atComponent"></param>
        /// <returns></returns>
        bool SubscribeTask(ACMethod acMethod, ACComponent atComponent);

        /// <summary>
        /// Unsubcribes a Task at Source-Component only if ACIdentifier of this IACPointAsyncRMISubscr is "TaskSubscriptionPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMISubscr&lt;T&gt;-Interface
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="atComponent"></param>
        /// <returns></returns>
        bool UnSubscribeTask(ACMethod acMethod, ACComponent atComponent);
    }
}

