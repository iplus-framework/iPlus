// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Config. export property-log'}de{'Konfig. export Eigenschaftslog'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PAFilePropertyLogConfig : PAPropRedirectConfigBase
    {
        public static void InitProperty(Database database, IACObject acObject)
        {
            if (acObject is ACClassProperty)
            {
                ACClassProperty acClassProperty = acObject as ACClassProperty;

                acClassProperty.ValueTypeACClass = database.ACClass.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.TACLRBaseTypes && c.ACIdentifier == Const.TNameString).First();
            }
        }

        private string _PartOfFileName;
        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Unique part of filename'}de{'Eindeutiger Teil des Dateinamens'}")]
        public string PartOfFileName 
        {
            get
            {
                return _PartOfFileName;
            }

            set
            {
                _PartOfFileName = value;
                OnPropertyChanged("PartOfFileName");
            }
        }

        public override string ACIdentifier => PartOfFileName;

        public override string ACCaption => PartOfFileName;

        int _ExportSortIndex = 0;
        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Export sort order'}de{'Export Sortierreihenfolge'}")]
        public int ExportSortIndex
        {
            get
            {
                return _ExportSortIndex;
            }
            set
            {
                _ExportSortIndex = value;
                OnPropertyChanged("ExportSortIndex");
            }
        }

        public override string ToString()
        {
            return this.PartOfFileName;
        }
    }
}
