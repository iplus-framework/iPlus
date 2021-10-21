using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointAsyncRMIProxy'}de{'ACPointAsyncRMIProxy'}", Global.ACKinds.TACClass)]
    internal class ACPointAsyncRMIProxy : ACPointNetAsyncRMIProxy<ACComponent>, IACPointAsyncRMI
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointAsyncRMIProxy()
            : this(null, null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointAsyncRMIProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }
        #endregion

        public void ClearMyInvocations(IACComponentTaskSubscr subscriber)
        {
            ACPointAsyncRMIHelper.ClearMyInvocations(this, subscriber);
        }

        /// <summary>Add a Task at Target-Component</summary>
        /// <param name="acMethod"></param>
        /// <param name="subscriber"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        [ACMethodInfo("", "", 9999)]
        public IACPointEntry AddTask(ACMethod acMethod, IACComponentTaskSubscr subscriber)
        {
            if (this.ACIdentifier != Const.TaskInvocationPoint)
                return null;
            return ACPointAsyncRMIHelper.AddTask(this, acMethod, subscriber);
        }

        /// <summary>Remove a Task at Target-Component</summary>
        /// <param name="acMethod"></param>
        /// <param name="subscriber"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        [ACMethodInfo("", "", 9999)]
        public bool RemoveTask(ACMethod acMethod, IACComponentTaskSubscr subscriber)
        {
            if (this.ACIdentifier != Const.TaskInvocationPoint)
                return false;
            return ACPointAsyncRMIHelper.RemoveTask(this, acMethod, subscriber);
        }
    }
}

