using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public interface IPAFuncTransportMaterial : IACComponentProcessFunction
    {
        ACStateEnum CurrentACState { get; }

        bool IsTransportActive { get; }
    }
}
