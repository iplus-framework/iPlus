using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACWorkCycle : IACComponent
    {
        bool IsSubscribedToWorkCycle { get; }
        void SubscribeToProjectWorkCycle();
        void UnSubscribeToProjectWorkCycle();
        bool IsEnabledSubscribeToProjectWorkCycle();
        bool IsEnabledUnSubscribeToProjectWorkCycle();
    }

    public interface IACWorkCycleWithACState : IACWorkCycle
    {
        ACStateEnum CurrentACState { get; }
        bool IsACStateInconsistent { get; }
    }
}
