﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IACEdge
    {
        IACPointBase Source { get; }
        IACPointBase Target { get; }

        IACObject SourceParent { get; }
        IACObject TargetParent { get; }
    }
}
