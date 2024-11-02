// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [DataContract]
    [ACSerializeableInfo(new Type[] { typeof(RuleValue) })]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RuleValue'}de{'RuleValue'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class RuleValue
    {

        [DataMember]
        public ACClassWFRuleTypes RuleType { get; set; } 	// =>	Excluded_module_types

        [DataMember]
        public List<string> ACClassACUrl { get; set; }

        [DataMember]
        public List<object> RuleObjectValue { get; set; }

        public override bool Equals(object obj)
        {
            bool isEqual = false;
            if(obj != null)
            {
                RuleValue second = obj as RuleValue;
                if(RuleType == second.RuleType)
                {
                    isEqual = (ACClassACUrl == null && second.ACClassACUrl == null)
                        ||( ACClassACUrl != null && second.ACClassACUrl != null && ACClassACUrl.Equals(second.ACClassACUrl));
                }
            }
            return isEqual;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public IEnumerable<ACClass> GetSelectedClasses(Database db)
        {
            if (ACClassACUrl == null || !ACClassACUrl.Any())
                return new ACClass[] { };
            else
            {
                List<ACClass> acClassList = new List<ACClass>();
                foreach (string acClassUrl in ACClassACUrl)
                {
                    string acUrl = acClassUrl;
                    if (acUrl.StartsWith(Const.ContextDatabase + "\\"))
                        acUrl = acUrl.Substring(9);

                    ACClass acClass = db.ACUrlCommand(acUrl) as ACClass;
                    if (acClass != null)
                    {
                        acClassList.Add(acClass);
                    }
                }
                return acClassList;
            }
        }
    }
}
