using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.manager
{
    public class WFLayoutCalculator
    {
        public WFLayoutCalculator()
        {
        }

        #region static
        public const int LeftSpace = 20;
        public const int RightSpace = 20;
        public const int HorzSpace = 20;
        public const int HeaderSpace = 20;
        public const int TopSpace = 3;
        public const int BottomSpace = 10;
        public const int VertSpace = 20;

        #endregion

        public Size SizeValue(int numOfElements)
        {
            Size size = new Size(200,200);
            if (numOfElements > 3 && numOfElements <= 6)
                size = new Size(400, 400);

            else if (numOfElements > 6 && numOfElements <= 9)
                size = new Size(500, 500);

            else if (numOfElements > 9)
                size = new Size(600, 600);

            return size;
        }

        
    }
}
