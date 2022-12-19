using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RuleValueList'}de{'RuleValueList'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    [DataContract]
    [ACSerializeableInfo(new Type[] {typeof(List<RuleValue>)})]
    public class RuleValueList
    {
        public const string ClassName = "RuleValueList";

        [DataMember]
        [ACPropertyInfo(1, "Items", "en{'Items'}de{'Items'}")]
        public List<RuleValue> Items { get; set; }

        public IEnumerable<ACClass> GetSelectedClasses(ACClassWFRuleTypes ruleType, Database db)
        {
            if (Items == null)
                return new ACClass[] { };

            var queryAllowedInstances = Items.Where(c => c.RuleType == ruleType);
            if (queryAllowedInstances.Any())
            {
                List<ACClass> selectedClasses = new List<ACClass>();
                foreach (var ruleValue in queryAllowedInstances)
                {
                    var result = ruleValue.GetSelectedClasses(db);
                    if (result != null && result.Any())
                        selectedClasses.AddRange(result);
                }
                return selectedClasses;
            }
            return new ACClass[] { };
        }

        public bool IsBreakPointSet()
        {
            if (Items == null)
                return false;

            var item = Items.FirstOrDefault();
            return 
                item != null && 
                item.RuleType == ACClassWFRuleTypes.Breakpoint && 
                item.RuleObjectValue != null && 
                item.RuleObjectValue.Any() && 
                item.RuleObjectValue.FirstOrDefault() is bool && 
                ((bool)item.RuleObjectValue.FirstOrDefault());
        }

        public bool Equals(RuleValueList obj)
        {
            if (obj == null)
                return false;
            if (Items == null)
                return false;
            if (this.Items.Count != obj.Items.Count)
                return false;
            foreach (var item in Items)
            {
                if (!obj.Items.Where(c => c.Equals(item)).Any())
                    return false;
            }
            return true;
        }
    }
}
