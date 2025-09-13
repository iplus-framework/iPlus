// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using gip.ext.designer.avui.Services;
using gip.ext.design.avui;
using System.Windows.Media;
using System.Windows.Data;
using gip.ext.designer.avui.Converters;

namespace gip.ext.designer.avui.Controls
{
	public class PanelMoveAdorner : Control
	{
		static PanelMoveAdorner()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PanelMoveAdorner),
				new FrameworkPropertyMetadata(typeof(PanelMoveAdorner)));
		}

        private ScaleTransform scaleTransform;

        public PanelMoveAdorner(DesignItem item)
        {
            this.item = item;

            scaleTransform = new ScaleTransform(1.0, 1.0);
            this.LayoutTransform = scaleTransform;
        }

        DesignItem item;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			e.Handled = true;
            item.Services.Selection.SetSelectedComponents(new DesignItem [] { item }, SelectionTypes.Auto);
			new DragMoveMouseGesture(item, false).Start(item.Services.DesignPanel, e);
		}

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var bnd = new Binding("IsVisible") { Source = item.Component };
            bnd.Converter = CollapsedWhenFalse.Instance;
            BindingOperations.SetBinding(this, UIElement.VisibilityProperty, bnd);

            var surface = this.TryFindParent<DesignSurface>();
            if (surface != null && surface.ZoomControl != null)
            {
                bnd = new Binding("CurrentZoom") { Source = surface.ZoomControl };
                bnd.Converter = InvertedZoomConverter.Instance;

                BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, bnd);
                BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, bnd);
            }
        }
    }
}
