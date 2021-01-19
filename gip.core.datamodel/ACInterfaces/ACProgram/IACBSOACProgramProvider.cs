using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    public interface IACBSOACProgramProvider : IACComponent
    {
        bool IsEnabledACProgram { get; set; }

        string WorkflowACUrl { get; }

        IEnumerable<ACProgram> GetACProgram();

        string GetProdOrderProgramNo();

        DateTime GetProdOrderInsertDate();
    }
}
