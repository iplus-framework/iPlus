using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui
{
    public interface IVBDynamicIcon
    {
        Brush ContentStroke
        {
            get;
            set;
        }

        Brush ContentFill
        {
            get;
            set;
        }
    }
}
