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
    /// <summary>
    /// Subscription Object Dispatched from Service
    /// </summary>
    [DataContract]
    public class ACPSubscrObjDispService : ACPSubscrObjBase
    {
        #region c'tors
        public ACPSubscrObjDispService()
            : base()
        {
        }

        public ACPSubscrObjDispService(string ACUrl)
            : base(ACUrl, null, true) // Autoattasch ist true, weil der Serializer auf die ChangedPointList-Liste zugreift. Jedoch wird nach dem Versand ein Detach aufgerufen
        {
        }

        // Konstruktor für Client-Seite: ACObject kann übergeben werden wenn Subscription oder Point-Änderungen gemacht werden, 
        // weil Subscription kurzzeitig lebt und durch die Referenz der Zugriff schneller ist.
        // Bei UnSubscribe, darf dieser Konstruktor nicht verwendet werden, weil das Objekt nicht vollständig gelöscht wird.
        // Ebenfalls darf dieser auf Server-Seite nicht verwendet werden, damit Objekte kurzzeitig Restartet werden können.
        public ACPSubscrObjDispService(ACComponent acObject)
            : base(acObject)
        {
        }
        #endregion

        #region Members
        /// <summary>
        /// Must be called when Subscription arrived
        /// </summary>
        internal void UpdatePointsOnProxyObjects()
        {
            if (UpdatePointList != null)
            {
                if (!this.IsAttached)
                    this.Attach();
                ACComponent refComponent = ParentACObject as ACComponent;
                if (refComponent != null)
                {
                    foreach (IACPointNetBase updatePoint in UpdatePointList)
                    {
                        updatePoint.RebuildAfterDeserialization(null);

                        IACPointNetBase existingPoint = null;
                        var acPointNetList = refComponent.ACPointNetList;
                        if (acPointNetList != null && acPointNetList.Any())
                            existingPoint = acPointNetList.Where(c => c.ACIdentifier == updatePoint.ACIdentifier).FirstOrDefault();
                        // TODO: Callback auf Proxy-Objekt ob Änderung erlaubt. Parameter:
                        // bool change = Delegate(updatePoint, existingPoint);

                        // Alter Punkt darf nicht entfernet werden, weil er direkt am Objekt hängt und Point als Member hat
                        // -> Kopiere Inhalt
                        if (existingPoint == null)
                            continue;
                        existingPoint.OnPointReceivedRemote(updatePoint);
                    }
                }
                this.Detach();

                // Rücksenden, falls aktualisiert und notwendig
            }
        }

        [IgnoreDataMember]
        protected List<IACPointNetBase> _ChangedPointList = null;
        [DataMember]
        public List<IACPointNetBase> ChangedPointList
        {
            // Abfrage von Serializer:
            get
            {
                return _ChangedPointList;
            }

            set
            {
                // Setzen von Serializer
                _ChangedPointList = value;
            }
        }

        public List<IACPointNetBase> UpdatePointList
        {
            // Abfrage von ACPSubscrACObjectServer bei Update
            get
            {
                return _ChangedPointList;
            }
        }

        #endregion
    }

}
