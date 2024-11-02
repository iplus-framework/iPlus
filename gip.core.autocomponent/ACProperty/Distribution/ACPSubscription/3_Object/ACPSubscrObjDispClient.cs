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
    /// Subscription Object Disptached from Client
    /// </summary>
    [DataContract]
    public class ACPSubscrObjDispClient : ACPSubscrObjBase
    {
        #region c'tors
        public ACPSubscrObjDispClient()
            : base()
        {
        }

        public ACPSubscrObjDispClient(string ACUrl)
            : base(ACUrl, null, true) // Autoattasch ist true, weil der Serializer auf die ChangedPointList-Liste zugreift. Jedoch wird nach dem Versand ein Detach aufgerufen
        {
        }

        // Konstruktor für Client-Seite: ACObject kann übergeben werden wenn Subscription oder Point-Änderungen gemacht werden, 
        // weil Subscription kurzzeitig lebt und durch die Referenz der Zugriff schneller ist.
        // Bei UnSubscribe, darf dieser Konstruktor nicht verwendet werden, weil das Objekt nicht vollständig gelöscht wird.
        // Ebenfalls darf dieser auf Server-Seite nicht verwendet werden, damit Objekte kurzzeitig Restartet werden können.
        public ACPSubscrObjDispClient(ACComponent acObject)
            : base(acObject)
        {
        }
        #endregion

        #region Members
        [IgnoreDataMember]
        protected List<IACPointNetBase> _ChangedPointList = null;
        [DataMember]
        public List<IACPointNetBase> ChangedPointList
        {
            // Abfrage von Serializer:
            get
            {
                // Falls schonmal abgefragt von Serializer, dann ist PointChangedForBroadcast-State weg, hole Info aus Liste heraus
                if (_ChangedPointList != null)
                    return _ChangedPointList;

                _ChangedPointList = new List<IACPointNetBase>();
                if (!this.IsAttached)
                    this.Attach();
                ACComponent refComponent = ParentACObject as ACComponent;
                if (refComponent != null)
                {
                    var acPointNetList = refComponent.ACPointNetList;
                    if (acPointNetList != null && acPointNetList.Any())
                    {
                        _ChangedPointList = acPointNetList.Where(c => c.PointChangedForBroadcast).ToList();
                        if (_ChangedPointList.Any())
                            _ChangedPointList.ForEach(c => c.PointChangedForBroadcast = false);
                    }
                }
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
            // Abfrage von ACPSubscrObjService bei Update
            get
            {
                return _ChangedPointList;
            }
        }

        #endregion
    }

}
