// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.designer.avui.Services;
using gip.ext.design.avui;
using gip.ext.designer.avui.Converters;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia;

namespace gip.ext.designer.avui.Controls
{
	public class PanelMoveAdorner : TemplatedControl
	{
		static PanelMoveAdorner()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(PanelMoveAdorner),
			//	new FrameworkPropertyMetadata(typeof(PanelMoveAdorner)));
		}

        private ScaleTransform scaleTransform;

        public PanelMoveAdorner(DesignItem item)
        {
            this.item = item;

            scaleTransform = new ScaleTransform(1.0, 1.0);
            this.RenderTransform = scaleTransform;
            // Old:
            //this.LayoutTransform = scaleTransform;
        }

        DesignItem item;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed)
            {
                e.Handled = true;
                item.Services.Selection.SetSelectedComponents(new DesignItem[] { item }, SelectionTypes.Auto);
                new DragMoveMouseGesture(item, false).Start(item.Services.DesignPanel, e);
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var bnd = new Binding("IsVisible") { Source = item.Component };
            bnd.Converter = CollapsedWhenFalse.Instance;
            this.Bind(Visual.IsVisibleProperty, bnd);

            var surface = this.TryFindParent<DesignSurface>();
            if (surface != null && surface.ZoomControl != null)
            {
                bnd = new Binding("CurrentZoom") { Source = surface.ZoomControl };
                bnd.Converter = InvertedZoomConverter.Instance;

                scaleTransform.Bind(ScaleTransform.ScaleXProperty, bnd);
                scaleTransform.Bind(ScaleTransform.ScaleYProperty, bnd);
            }
        }
    }
}
