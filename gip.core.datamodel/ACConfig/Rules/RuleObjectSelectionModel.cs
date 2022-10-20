using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class RuleObjectSelectionModel
    {
        public RuleInfo RuleInfo { get; set; }
        public string ConfigStoreURL { get; set; }
        public string PreACUrl { get; set; }
        public string PAFRulePropertyACUrl { get; set; }
        public List<object> AvailableValues { get; set; }
        public List<object> SelectedValues { get; set; }
    }
}
