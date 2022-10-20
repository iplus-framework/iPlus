using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class RuleTypeDefinition
    {
        public List<Global.ACKinds> RuleApplyedWFACKindTypes { get; set; }
        public ACClassWFRuleTypes RuleType { get; set; }
        public string Translation { get; set; }
    }
}
