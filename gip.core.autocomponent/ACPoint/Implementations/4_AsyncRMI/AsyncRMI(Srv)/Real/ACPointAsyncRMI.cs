using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointAsyncRMI'}de{'ACPointAsyncRMI'}", Global.ACKinds.TACClass)]
    public class ACPointAsyncRMI : ACPointNetAsyncRMI<ACComponent>, IACPointAsyncRMI
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointAsyncRMI()
            : this(null, (ACClassProperty)null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointAsyncRMI(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
        }

        public ACPointAsyncRMI(IACComponent parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }

        #endregion

        #region Methods
        public void ClearMyInvocations(IACComponentTaskSubscr subscriber)
        {
            ACPointAsyncRMIHelper.ClearMyInvocations(this, subscriber);
        }

        /// <summary>
        /// Add a Task at Target-Component only if ACIdentifier of this IACPointAsyncRMI is "TaskInvocationPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMI&lt;T&gt;-Interface
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        [ACMethodInfo("", "", 9999)]
        public IACPointEntry AddTask(ACMethod acMethod, IACComponentTaskSubscr subscriber)
        {
            if (this.ACIdentifier != Const.TaskInvocationPoint)
                return null;
            return ACPointAsyncRMIHelper.AddTask(this, acMethod, subscriber);
        }

        /// <summary>
        /// Remove a Task at Target-Component only if ACIdentifier of this IACPointAsyncRMI is "TaskInvocationPoint"
        /// Else use InvokeAsyncMethod of IACPointAsyncRMI&lt;T&gt;-Interface
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        [ACMethodInfo("", "", 9999)]
        public bool RemoveTask(ACMethod acMethod, IACComponentTaskSubscr subscriber)
        {
            if (this.ACIdentifier != Const.TaskInvocationPoint)
                return false;
            return ACPointAsyncRMIHelper.RemoveTask(this, acMethod, subscriber);
        }

        public int CheckAndRemoveAllDeadTasks()
        {
            int removedCount = 0;
            IEnumerable<ACPointAsyncRMIWrap<ACComponent>> query = null;

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                query = LocalStorage.ToArray();
            }

            if (query != null && query.Any())
            {
                foreach (var wrapObject in query)
                {
                    ACComponent subscriber = wrapObject.ValueT;
                    if (subscriber == null)
                    {
                        Remove(wrapObject);
                        removedCount++;
                    }
                    else if (!String.IsNullOrEmpty(wrapObject.ClientPointName))
                    {
                        IACPointBase clientPoint = subscriber.GetPointNet(wrapObject.ClientPointName);
                        if ((clientPoint != null) && (clientPoint is IACPointAsyncRMISubscr<ACComponent>))
                        {
                            IACPointAsyncRMISubscr<ACComponent> clientSubscrRMIPoint = clientPoint as IACPointAsyncRMISubscr<ACComponent>;
                            if (!clientSubscrRMIPoint.Contains(wrapObject))
                            {
                                Remove(wrapObject);
                                removedCount++;
                            }
                        }
                    }
                }
            }
            return removedCount;
        }

        #endregion
    }
}

