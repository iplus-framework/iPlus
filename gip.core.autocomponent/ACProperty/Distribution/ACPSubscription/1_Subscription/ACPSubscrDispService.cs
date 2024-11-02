// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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
    public class ACPSubscrDispService : ACPSubscrDispBase
    {
        #region c'tors
        public ACPSubscrDispService()
            : base()
        {
        }
        #endregion

        #region Members
        #endregion

        #region Methods
        protected override ACPSubscrProjBase CreateNewSubscriptionProject(string acName)
        {
            return new ACPSubscrProjDispService(acName);
        }

        /// <summary>
        /// Must be called when Subscription arrived
        /// </summary>
        internal void UpdatePointsOnProxyObjects()
        {
            foreach (ACPSubscrProjDispService subscrProj in _ProjectSubscriptionList)
            {
                subscrProj.UpdatePointsOnProxyObjects();
            }
        }

        #endregion
    }
}
