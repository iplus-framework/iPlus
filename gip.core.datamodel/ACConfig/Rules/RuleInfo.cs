using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class RuleInfo : INotifyPropertyChanged
    {
        private readonly ACClassWFRuleTypes _RuleType;
        private readonly string _Caption;
        private string _ConfigStoreName;

        public bool _IsReadOnly = true;

        public event PropertyChangedEventHandler PropertyChanged;
        //private ACClassWF _CurrentACClassWF;
        //private ACClassWFRuleTypes aCClassWFRuleTypes;
        //private string p;

        #region - Properties

        public ACClassWFRuleTypes RuleType
        {
            get { return _RuleType; }
        }

        public string Caption
        {
            get { return Translator.GetTranslation(_Caption); }
        }


        public string ConfigStoreName
        {
            get
            {
                return _ConfigStoreName;
            }
        }

        public string ConfigStoreUrl { get; set; }

        
        public bool IsDefined
        {
            get
            {
                return false;
                //if (_KeyInfo == null) return _Host.ACClassWFRule_ACClassWF.Where(r => r.ACRuleType == _RuleType && r.RuleKey == null).Any();
                //else return _Host.ACClassWFRule_ACClassWF.Where(r => r.ACRuleType == _RuleType && r.RuleKey == _KeyInfo.Item1).Any();
            }
        }

        public bool IsReadOnly
        {
            get { return _IsReadOnly; }
        }

        #endregion

        #region - Constructors

        public RuleInfo(ACClassWFRuleTypes ruleType, string caption, IACConfigStore configStore, bool isReadOnly = true)
        {
            _RuleType = ruleType;
            _Caption = caption;
            _IsReadOnly = isReadOnly;
            _ConfigStoreName = configStore.ConfigStoreName;
            ConfigStoreUrl = configStore.GetACUrl();
        }
       

        #endregion

        #region - Methods

        public void RefreshIsDefined()
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IsDefined"));
        }

        public override string ToString()
        {
            return this.ConfigStoreName;
        }

        #endregion

    }

}
