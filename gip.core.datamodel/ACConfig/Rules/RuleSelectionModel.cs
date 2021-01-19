using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// 
    /// </summary>
    public class RuleSelectionModel : INotifyPropertyChanged
    {
        [ACPropertyList(9999, "RuleType")]
        public RuleInfo[] RuleTypeList
        {
            get { return null; }
        }

        private RuleInfo _CurrentRuleType;

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        [ACPropertyCurrent(9999, "RuleType", "en{'Rule'}de{'Rule'}")]
        public RuleInfo CurrentRuleType
        {
            get
            {
                return _CurrentRuleType;
            }
            set
            {

                if (_CurrentRuleType != value)
                {
                    _CurrentRuleType = value;
                    OnPropertyChanged("CurrentRuleType");
                    OnPropertyChanged("IsMultiValueRuleType");
                    //if (_CurrentRuleType != null && _CurrentRuleType.RuleType == ACClassWFRuleTypes.ActiveRoutes)
                    //    InitRoutingValues();
                }
                else
                {
                    OnPropertyChanged("SelectedValues");
                    OnPropertyChanged("AvailableValues");
                }
            }
        }


        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public List<RuleSelectionItem> Items { get; set; }
    }
}
