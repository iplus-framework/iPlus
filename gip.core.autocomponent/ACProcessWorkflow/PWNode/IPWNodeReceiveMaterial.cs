using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public interface IPWNodeReceiveMaterial :  IACComponent
    {
        /// <summary>
        /// Check if has any material to process (dosing, weighing...)
        /// </summary>
        bool HasAnyMaterialToProcess { get; }
    }

    public interface IPWNodeReceiveMaterialRouteable : IPWNodeReceiveMaterial
    {
        Route CurrentDosingRoute { get; set; }
    }
}
