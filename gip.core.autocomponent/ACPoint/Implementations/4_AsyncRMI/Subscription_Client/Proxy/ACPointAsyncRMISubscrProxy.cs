using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointAsyncRMISubscrProxy'}de{'ACPointAsyncRMISubscrProxy'}", Global.ACKinds.TACClass)]
    internal sealed class ACPointAsyncRMISubscrProxy : ACPointNetAsyncRMISubscrProxy<ACComponent>, IACPointAsyncRMISubscr
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointAsyncRMISubscrProxy()
            : this(null, null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointAsyncRMISubscrProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }
        #endregion

        /// <summary>
        /// Subscribes a Task at Source-Component only if ACIdentifier of this IACPointAsyncRMISubscr is "TaskSubscriptionPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMISubscr&lt;T&gt;-Interface
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="atComponent"></param>
        /// <returns></returns>
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
        /// <returns></returns>
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
