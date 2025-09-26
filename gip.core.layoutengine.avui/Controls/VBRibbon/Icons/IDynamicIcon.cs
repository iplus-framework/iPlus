using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui
{
    public interface IVBDynamicIcon
    {
        IBrush ContentStroke
        {
            get;
            set;
        }

        IBrush ContentFill
        {
            get;
            set;
        }
    }
}
