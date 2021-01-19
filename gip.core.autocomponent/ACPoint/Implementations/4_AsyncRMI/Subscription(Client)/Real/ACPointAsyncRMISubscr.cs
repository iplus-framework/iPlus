using System;
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

        /// <summary>
        /// Constructor for Reflection-Instantiation
        /// </summary>
        /// <param name="parent"></param>
        public ACPointAsyncRMISubscr(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }

        public ACPointAsyncRMISubscr(IACComponent parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }

        #endregion

        /// <summary>
        /// Subscribes a Task at Source-Component only if ACIdentifier of this IACPointAsyncRMISubscr is "TaskSubscriptionPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMISubscr<T>-Interface
        /// </summary>
        /// <param name="acMethod"></param>
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
        /// Else use InvokeAsyncMethod of IACPointAsyncRMISubscr<T>-Interface
        /// </summary>
        /// <param name="acMethod"></param>
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

