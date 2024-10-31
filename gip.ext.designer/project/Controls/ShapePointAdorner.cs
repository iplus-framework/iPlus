// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.core.datamodel;
using gip.ext.design;
using gip.ext.design.Adorners;
using gip.ext.design.Extensions;
using gip.ext.designer.Extensions;
using gip.ext.designer.Services;

namespace gip.ext.designer.Controls
{
    public class ShapePointAdorner : Control
    {
        static ShapePointAdorner()
		{
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ShapePointAdorner), new FrameworkPropertyMetadata(typeof(ShapePointAdorner)));
		}

        public readonly DesignItem _shapeDesignItem;
        public readonly Point _pointToAdorn;

        public ShapePointAdorner(DesignItem shapeDesignItem, Point pointToAdorn)
        {
            this._shapeDesignItem = shapeDesignItem;
            this._pointToAdorn = pointToAdorn;
        }
    }

}
