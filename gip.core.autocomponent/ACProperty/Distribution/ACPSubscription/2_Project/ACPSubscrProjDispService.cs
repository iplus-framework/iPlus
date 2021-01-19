using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// ACPSubscrProjDisp verwaltet eine Liste von dem Client abbonierten ACObjects
    /// </summary>
    [DataContract]
    public class ACPSubscrProjDispService : ACPSubscrProjBase
    {
        #region c'tors
        public ACPSubscrProjDispService(string acName)
            : base(acName)
        {
        }
        #endregion

        #region Members
        #endregion

        #region Methods
        protected override ACPSubscrObjBase CreateNewSubscriptionObject(string ACUrl)
        {
            return new ACPSubscrObjDispService(ACUrl);
        }

        protected override void OnRemoveSubscriptionObject(ACPSubscrObjBase subscrObj)
        {
        }

        /// <summary>
        /// Must be called when Subscription arrived
        /// </summary>
        internal void UpdatePointsOnProxyObjects()
        {
            foreach (ACPSubscrObjDispService subscrObj in SubscribedACObjects)
            {
                subscrObj.UpdatePointsOnProxyObjects();
            }
        }

        #endregion
    }
}
