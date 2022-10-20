using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public class ComponentPWNodeConfigModel
    {
        public gip.core.datamodel.ACClassWF ContentACClassWF { get; set; }

        public string PreACUrl { get; set; }

        public gip.core.datamodel.ACClassMethod PWNodeMethod { get; set; }
        public string PWNodeLocalConfigACUrl { get; set; }
        public List<ACConfigParam> PWNodeParams { get; set; }

        public gip.core.datamodel.ACClassMethod PAFunctionMethod { get; set; }
        public string PAFunctionLocalConfigACUrl { get; set; }
        public List<ACConfigParam> PAFunctionParams { get; set; }


        public Dictionary<ACClassWFRuleTypes, List<object>> Rules { get; set; }
    }
}
