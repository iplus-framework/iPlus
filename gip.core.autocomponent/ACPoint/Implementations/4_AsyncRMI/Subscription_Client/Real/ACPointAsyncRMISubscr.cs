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
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointAsyncRMISubscr'}de{'ACPointAsyncRMISubscr'}", Global.ACKinds.TACClass)]
    public sealed class ACPointAsyncRMISubscr : ACPointNetAsyncRMISubscrBase<ACComponent>, IACPointAsyncRMISubscr
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointAsyncRMISubscr()
            : this(null, (ACClassProperty)null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointAsyncRMISubscr(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ACPointAsyncRMISubscr" /> class.</summary>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="maxCapacity">The maximum capacity.</param>
        public ACPointAsyncRMISubscr(IACComponent parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }

        #endregion

        /// <summary>
        /// Subscribes a Task at Source-Component only if ACIdentifier of this IACPointAsyncRMISubscr is "TaskSubscriptionPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMISubscr&lt;T&gt;-Interface
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="atComponent"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        [ACMethodInfo("", "", 9999)]
        public bool SubscribeTask(ACMethod acMethod, ACComponent atComponent)
        {
            if (this.ACIdentifier != Const.TaskSubscriptionPoint)
                return false;
            return ACPointAsyncRMIHelper.SubscribeTask(this, acMethod, atComponent);
        }

        /// <summary>
        /// Unsubcribes a Task at Source-Component only if ACIdentifier of this IACPointAsyncRMISubscr is "TaskSubscriptionPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMISubscr&lt;T&gt;-Interface
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="atComponent"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        [ACMethodInfo("", "", 9999)]
        public bool UnSubscribeTask(ACMethod acMethod, ACComponent atComponent)
        {
            if (this.ACIdentifier != Const.TaskSubscriptionPoint)
                return false;
            return ACPointAsyncRMIHelper.UnSubscribeTask(this, acMethod, atComponent);
        }

        public override void UnSubscribe()
        {
            if (this.ACIdentifier != Const.TaskSubscriptionPoint)
                return;
            ACPointAsyncRMIHelper.UnSubscribeAll(this);
        }
    }
}

