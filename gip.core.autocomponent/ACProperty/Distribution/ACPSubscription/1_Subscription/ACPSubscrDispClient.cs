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
    public class ACPSubscrDispClient : ACPSubscrDispBase
    {
        #region c'tors
        public ACPSubscrDispClient()
            : base()
        {
        }
        #endregion

        #region Members
        #endregion

        #region Methods
        protected override ACPSubscrProjBase CreateNewSubscriptionProject(string acName)
        {
            return new ACPSubscrProjDispClient(acName);
        }

        #endregion
    }
}
