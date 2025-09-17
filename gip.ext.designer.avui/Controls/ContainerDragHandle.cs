// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using gip.ext.design.avui;
using gip.ext.designer.avui.Converters;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// A thumb where the look can depend on the IsPrimarySelection property.
	/// Used by UIElementSelectionRectangle.
	/// </summary>
	public class ContainerDragHandle : TemplatedControl
    {
		static ContainerDragHandle()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(ContainerDragHandle), new StyledPropertyMetadata<Type>(typeof(ContainerDragHandle)));
		}

        private ScaleTransform scaleTransform;

        public ContainerDragHandle()
        {
            scaleTransform = new ScaleTransform(1.0, 1.0);
            this.RenderTransform = scaleTransform;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {

            var surface = this.TryFindParent<DesignSurface>();
            if (surface != null && surface.ZoomControl != null)
            {
                var bnd = new Binding("CurrentZoom") { Source = surface.ZoomControl };
                bnd.Converter = InvertedZoomConverter.Instance;

                scaleTransform.Bind(ScaleTransform.ScaleXProperty, bnd);
                scaleTransform.Bind(ScaleTransform.ScaleYProperty, bnd);
            }

            base.OnApplyTemplate(e);

        }
    }
}
