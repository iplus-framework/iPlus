using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class RuleSelectionItem
    {

        public RuleInfo _CurrentRuleType;

        public string Title { get; set; }

        public ACClassWF CurrentACClassWF { get; set; }

        public string PreConfigACUrl { get; set; }
        public string LocalConfigACUrl { get; set; }

        private List<RuleObjectSelectionModel> _RuleObjectSelection;
        public List<RuleObjectSelectionModel> RuleObjectSelection
        {
            get
            {
                if (_RuleObjectSelection == null)
                    _RuleObjectSelection = new List<RuleObjectSelectionModel>();
                return _RuleObjectSelection;
            }
        }

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

        public bool IsMultiValueRuleType
        {
            get { return _CurrentRuleType != null && _CurrentRuleType.RuleType != ACClassWFRuleTypes.Parallelization && _CurrentRuleType.RuleType != ACClassWFRuleTypes.Breakpoint; }
        }
    }
}
