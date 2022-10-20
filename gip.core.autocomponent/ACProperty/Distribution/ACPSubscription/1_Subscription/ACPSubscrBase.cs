using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.Threading;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Für jeden Abbonenten im Netzwerk, wird auf Server-Seite eine oder mehrere Instanzen von einem Observer angelegt
    /// Mit der Subscription wird registriert, an welchen Objekten der Client interessiert ist
    /// In bestimmten Zyklen, muss die ACPropertyDispatchProject-Liste geleert werden, 
    /// indem alle Subscriptions mit dem zu versendenten Inhalt informiert werden
    /// </summary>
    [DataContract]
    public abstract class ACPSubscrBase
    {
        #region c'tors
        public ACPSubscrBase()
        {
        }
        #endregion

        #region Members

        [DataMember(Name = "PSL")]
        protected List<ACPSubscrProjBase> _ProjectSubscriptionList = new List<ACPSubscrProjBase>();

        [IgnoreDataMember]
        public List<ACPSubscrProjBase> ProjectSubscriptionList
        {
            get
            {
                return _ProjectSubscriptionList;
            }
        }
        #endregion

        #region Methods

        public ACPSubscrProjBase GetProject(string ProjectACIdentifier, bool withCS)
        {

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                return _ProjectSubscriptionList.Where(c => c.ACIdentifier == ProjectACIdentifier).FirstOrDefault();
            }
        }


        /// <summary>
        /// Enters Critical Section e.g. if access to Childs-List which could be emptied from framework during access
        /// </summary>
        public bool TryEnterCS()
        {
            int tries = 0;
            while (tries < 100)
            {
                if (ACMonitor.TryEnter(_20054_LockProjectSubscriptionList, 10))
                    break;
                tries++;
            }
            return tries < 100;
        }

        //public void EnterCS()
        //{
        //    ACMonitor.Enter(_LockProjectSubscriptionList);
        //}

        /// <summary>
        /// Leaves Critical Section
        /// </summary>
        public void LeaveCS()
        {
            ACMonitor.Exit(_20054_LockProjectSubscriptionList);
        }

        internal readonly ACMonitorObject _20054_LockProjectSubscriptionList = new ACMonitorObject(20054);


        public void Detach()
        {

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                if (ProjectSubscriptionList != null)
                    ProjectSubscriptionList.ForEach(c => c.Detach());
            }
        }
#endregion

    }

}
