using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Rule types'}", Global.ACKinds.TACEnum)]
    public enum ACClassWFRuleTypes : short
    {
        Parallelization = 0,
        Allowed_instances = 10,
        ActiveRoutes = 20,
        Excluded_module_types = 30,
        Excluded_process_modules = 40,
        Breakpoint = 50,
    }
}
