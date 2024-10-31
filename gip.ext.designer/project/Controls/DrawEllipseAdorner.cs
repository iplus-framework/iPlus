// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using gip.ext.designer.Services;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.ext.design;

namespace gip.ext.designer.Controls
{
    public class DrawEllipseAdorner : DrawShapesAdornerBase
    {
        #region c'tors
        public DrawEllipseAdorner(Point startPointRelativeToShapeContainer, DesignItem containerOfStartPoint, DesignItem containerForShape)
            : base(startPointRelativeToShapeContainer, containerOfStartPoint, containerForShape)
        {
        }
        #endregion

        public override Geometry GetGeometry(Point pointRelativeToPathContainer)
        {
            return new EllipseGeometry(new Rect(StartPointRelativeToShapeContainer, pointRelativeToPathContainer));
        }

        public override DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, MouseButtonEventArgs e = null)
        {
            Ellipse newInstance = (Ellipse)ContainerForShape.Context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(Ellipse), null);
            DesignItem item = ContainerForShape.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            if ((item != null) && (item.View != null))
            {
                ApplyDefaultPropertiesToItem(item);
                item.Properties[Ellipse.WidthProperty].SetValue((this.Geometry as EllipseGeometry).Bounds.Width);
                item.Properties[Ellipse.HeightProperty].SetValue((this.Geometry as EllipseGeometry).Bounds.Height);
            }
            return item;
        }
    }
}
