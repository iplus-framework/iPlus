// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Config. redirection'}de{'Konfig. Umleitung'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PAPropRedirectConfig : PAPropRedirectConfigBase
    {
        public static void InitProperty(Database database, IACObject acObject)
        {
            if (acObject is ACClassProperty)
            {
                ACClassProperty acClassProperty = acObject as ACClassProperty;
                acClassProperty.ValueTypeACClass = database.ACClass.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.TACLRBaseTypes && c.ACIdentifier == Const.TNameString).First();
            }
        }

        private string _TargetUrl;
        [DataMember]
        [ACPropertyInfo(9999, "", "en{'ACUrl of Target-Property'}de{'ACUrl von Zieleigenschaft'}")]
        public string TargetUrl 
        {
            get
            {
                return _TargetUrl;
            }

            set
            {
                _TargetUrl = value;
                OnPropertyChanged("TargetUrl");
            }
        }

        public override string ACIdentifier => TargetUrl;

        public override string ACCaption => TargetUrl;

        public override string ToString()
        {
            return this.TargetUrl;
        }
    }
}
