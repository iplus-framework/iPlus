﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.manager
{
    public interface ITaskPreviewCall
    {

        void PreviewTask(ACClass applicationManager, ACClassTask task);
    }
}
