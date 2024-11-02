// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'ExportItemPreviewModel'}de{'ExportItemPreviewModel'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class CloneDialogModel
    {
        #region ctor's

        public CloneDialogModel()
        {
            IsCloneACClassProperty = true;
            IsCloneACClassMethod = true;
            IsCloneACClassDesign = true;
            IsCloneACClassConfig = true;
            IsCloneACClassText = true;
            IsCloneACClassMessage = true;
            IsCloneACClassPropertyRelation = true;
        }

        #endregion

        #region Properties

        [ACPropertyInfo(1, "ACIdentifier", "en{'New ACIdentifier'}de{'Neu ACIdentifier'}")]
        public string ACIdentifier { get; set; }

        [ACPropertyInfo(2, "IsCloneACClassProperty", "en{'Clone Properties'}de{'Klone Eigenschaften'}")]
        public bool IsCloneACClassProperty { get; set; }

        [ACPropertyCurrent(3, "IsCloneACClassMethod", "en{'Clone Methods'}de{'Klone Methoden'}")]
        public bool IsCloneACClassMethod { get; set; }

        [ACPropertyCurrent(4, "IsCloneACClassDesign", "en{'Clone Designs'}de{'Klone Designs'}")]
        public bool IsCloneACClassDesign { get; set; }

        [ACPropertyCurrent(5, "IsCloneACClassConfig", "en{'Clone Classconfig'}de{'Klone Klassenkonfiguration'}")]
        public bool IsCloneACClassConfig { get; set; }

        [ACPropertyCurrent(6, "IsCloneACClassText", "en{'Clone Texts'}de{'Klone Texte'}")]
        public bool IsCloneACClassText { get; set; }

        [ACPropertyCurrent(7, "IsCloneACClassMessage", "en{'Clone Messages'}de{'Klone Meldungen'}")]
        public bool IsCloneACClassMessage { get; set; }

        [ACPropertyCurrent(8, "IsCloneACClassPropertyRelation", "en{'Clone Propertyrelation'}de{'Klone Eigenschaftsbeziehung'}")]
        public bool IsCloneACClassPropertyRelation { get; set; }

        #endregion
    }
}
