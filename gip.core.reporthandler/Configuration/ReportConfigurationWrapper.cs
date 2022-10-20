using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace gip.core.reporthandler.Configuration
{
    /// <summary>
    /// The wrapper for report configuration.
    /// </summary>
    [Serializable]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ReportConfiguration'}de{'Bericht configuration'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ReportConfigurationWrapper
    {
        [ACPropertyInfo(999)]
        public ACClassWF ConfigACClassWF
        {
            get;
            set;
        }

        private List<IACConfig> _ConfigItems;
        [ACPropertyInfo(999)]
        public List<IACConfig> ConfigItems
        {
            get
            {
                if (_ConfigItems == null)
                    _ConfigItems = new List<IACConfig>();
                return _ConfigItems;
            }
            set
            {
                _ConfigItems = value;
            }
        }

        [ACPropertyInfo(999)]
        public string ConfigACUrl 
        {
            get
            {
                string pre = null;
                if (ConfigItems.Any())
                    pre = ConfigItems.FirstOrDefault().PreConfigACUrl;
                return pre + ConfigACClassWF.ConfigACUrl;
            }
        }

        public bool Configuration
        {
            get;
            set;
        }

        public bool Method
        {
            get;
            set;
        }

        public bool Rules
        {
            get;
            set;
        }
    }
}
