using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public interface IPWNodeReceiveMaterial : IACComponentPWNode
    {
        /// <summary>
        /// Check if has any material to process (dosing, weighing...)
        /// </summary>
        bool HasAnyMaterialToProcess { get; }

        bool HasRunSomeDosings { get; }

        int CountRunDosings { get; }

        bool HasDosedComponents(out double sumQuantity);

        bool HasOpenDosings(out double sumQuantity);

        bool HasAnyDosings(out double sumQuantity);

        void OnDosingLoopDecision(IACComponentPWNode dosingloop, bool willRepeatDosing);
    }

    public interface IPWNodeReceiveMaterialRouteable : IPWNodeReceiveMaterial
    {
        Route CurrentDosingRoute { get; set; }
    }
}
