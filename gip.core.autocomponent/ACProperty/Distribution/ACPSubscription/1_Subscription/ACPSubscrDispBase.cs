// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
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
    public abstract class ACPSubscrDispBase : ACPSubscrBase
    {
        #region c'tors
        public ACPSubscrDispBase()
            : base()
        {
        }
        #endregion

        #region Members
        #endregion

        #region Methods
        protected abstract ACPSubscrProjBase CreateNewSubscriptionProject(string acName);

        public bool Subscribe(IACComponent acComponentProject, IACComponent ChildNode)
        {
            if ((acComponentProject == null) || (ChildNode == null))
                return false;

            bool succeeded = false;

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                ACPSubscrProjBase subscrProject = GetProject(acComponentProject.ACIdentifier, false);
                if (subscrProject == null)
                {
                    subscrProject = CreateNewSubscriptionProject(acComponentProject.ACIdentifier);
                    _ProjectSubscriptionList.Add(subscrProject);
                }
                succeeded = (subscrProject.Subscribe(ChildNode.ACUrl) != null);
            }
            return succeeded;
        }


        public int UnSubscribe(IACComponent acComponentProject, IACComponent ChildNode)
        {
            if ((acComponentProject == null) || (ChildNode == null))
                return 0;

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                ACPSubscrProjBase subscrProject = GetProject(acComponentProject.ACIdentifier, true);
                if (subscrProject != null)
                    return subscrProject.UnSubscribe(ChildNode.ACUrl);
            }
            return 0;
        }

        public int UnSubscribe(IACComponent acComponentProject)
        {
            int nCount = 0;

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                nCount = _ProjectSubscriptionList.RemoveAll(x => x.ACIdentifier == acComponentProject.ACIdentifier);
            }
            return nCount;
        }

        public void UnSubscribeAll()
        {

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                _ProjectSubscriptionList = new List<ACPSubscrProjBase>();
            }
        }

#endregion
    }
}
