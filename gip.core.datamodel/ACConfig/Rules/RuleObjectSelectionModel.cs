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

        private List<object> _AvailableValues;
        public List<object> AvailableValues
        {
            get
            {
                return _AvailableValues;
            }
            set
            {
                _AvailableValues = value;
            }
        }

        private List<object> _SelectedValues;
        public List<object> SelectedValues 
        {
            get
            {
                return _SelectedValues;
            }
            set
            {
                _SelectedValues = value;
            }
        }
    }
}
