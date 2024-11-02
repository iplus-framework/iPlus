// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPropertyLogRule'}de{'ACPropertyLogRule'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true, "", "")]
    [ACPropertyEntity(1, "RuleType", "en{'Rule type'}de{'Regeltyp'}")]
    [ACPropertyEntity(3, ACClass.ClassName, "en{'ACClass'}de{'ACClass'}", Database.ClassName + "\\" + ACClass.ClassName + Const.DBSetAsEnumerablePostfix)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACPropertyLogRule.ClassName, "en{'ACPropertyLog'}de{'ACPropertyLog'}", typeof(ACPropertyLogRule), ACPropertyLogRule.ClassName, "", "")]
    [NotMapped]
    public partial class ACPropertyLogRule
    {
        public const string ClassName = "ACPropertyLogRule";

        public static ACPropertyLogRule NewACObject (Database db, ACClass acClass, Global.PropertyLogRuleType ruleType = Global.PropertyLogRuleType.ProjectHierarchy)
        {
            ACPropertyLogRule entityObject = new ACPropertyLogRule();
            entityObject.ACPropertyLogRuleID = Guid.NewGuid();
            entityObject.RuleType = (short)ruleType;
            entityObject.ACClassID = acClass.ACClassID;
            entityObject.SetInsertAndUpdateInfo(db.UserName, db);
            return entityObject;
        }
    }
}
