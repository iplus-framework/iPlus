﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPropertyLogRule'}de{'ACPropertyLogRule'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true, "", "")]
    [ACPropertyEntity(1, "RuleType", "en{'Rule type'}de{'Regeltyp'}")]
    [ACPropertyEntity(3, ACClass.ClassName, "en{'ACClass'}de{'ACClass'}", Database.ClassName + "\\" + ACClass.ClassName)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACPropertyLogRule.ClassName, "en{'ACPropertyLog'}de{'ACPropertyLog'}", typeof(ACPropertyLogRule), ACPropertyLogRule.ClassName, "", "")]
    public partial class ACPropertyLogRule
    {
        public const string ClassName = "ACPropertyLogRule";

        public static ACPropertyLogRule NewACObject (Database db, ACClass acClass, Global.PropertyLogRuleType ruleType = Global.PropertyLogRuleType.ProjectHierarchy)
        {
            ACPropertyLogRule entityObject = new ACPropertyLogRule();
            entityObject.ACPropertyLogRuleID = Guid.NewGuid();
            entityObject.RuleType = (short)ruleType;
            entityObject.ACClassID = acClass.ACClassID;
            entityObject.SetInsertAndUpdateInfo(Database.Initials, db);
            return entityObject;
        }
    }
}
