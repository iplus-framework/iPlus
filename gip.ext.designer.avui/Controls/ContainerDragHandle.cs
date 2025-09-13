// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// A thumb where the look can depend on the IsPrimarySelection property.
	/// Used by UIElementSelectionRectangle.
	/// </summary>
	public class ContainerDragHandle : Control
	{
		static ContainerDragHandle()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ContainerDragHandle), new FrameworkPropertyMetadata(typeof(ContainerDragHandle)));
		}

        private ScaleTransform scaleTransform;

        public ContainerDragHandle()
        {
            scaleTransform = new ScaleTransform(1.0, 1.0);
            this.LayoutTransform = scaleTransform;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var surface = this.TryFindParent<DesignSurface>();
            if (surface != null && surface.ZoomControl != null)
            {
                var bnd = new Binding("CurrentZoom") { Source = surface.ZoomControl };
                bnd.Converter = InvertedZoomConverter.Instance;

                BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, bnd);
                BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, bnd);
            }
        }
    }
}
