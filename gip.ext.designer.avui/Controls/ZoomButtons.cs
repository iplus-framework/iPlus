// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using gip.ext.design.avui;
using System;
using System.Collections.Generic;
using System.Text;

namespace gip.ext.designer.avui.Controls
{
	public class ZoomButtons : RangeBase
	{
		static ZoomButtons()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomButtons), new FrameworkPropertyMetadata(typeof(ZoomButtons)));
		}

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var uxPlus = e.NameScope.Find<Button>("uxPlus");
			var uxMinus = e.NameScope.Find<Button>("uxMinus");
			var uxReset = e.NameScope.Find<Button>("uxReset");
            var ux100Percent = e.NameScope.Find<Button>("ux100Percent");

            if (uxPlus != null)
                uxPlus.Click += OnZoomInClick; ;
			if (uxMinus != null)
				uxMinus.Click += OnZoomOutClick;
			if (uxReset != null)
				uxReset.Click += OnResetClick;
            if (ux100Percent != null)
                ux100Percent.Click += On100PercentClick;
        }

        const double ZoomFactor = 1.1;
		
		void OnZoomInClick(object sender, RoutedEventArgs e)
		{
			SetCurrentValue(ValueProperty, ZoomScrollViewer.RoundToOneIfClose(this.Value * ZoomFactor));
		}
		
		void OnZoomOutClick(object sender, RoutedEventArgs e)
		{
			SetCurrentValue(ValueProperty, ZoomScrollViewer.RoundToOneIfClose(this.Value / ZoomFactor));
		}
		
		void OnResetClick(object sender, RoutedEventArgs e)
		{
			SetCurrentValue(ValueProperty, 1.0);
		}

        void On100PercentClick(object sender, RoutedEventArgs e)
        {
            ZoomControl zctl = this.TryFindParent<ZoomControl>();
            var contentWidth = ((Control)zctl.Content).Bounds.Width;
            var contentHeight = ((Control)zctl.Content).Bounds.Height;
            var width = zctl.Bounds.Width;
            var height = zctl.Bounds.Height;

            if (contentWidth > width || contentHeight > height)
            {
                double widthProportion = contentWidth / width;
                double heightProportion = contentHeight / height;

                if (widthProportion > heightProportion)
                    SetCurrentValue(ValueProperty, (width - 20.00) / contentWidth);
                else
                    SetCurrentValue(ValueProperty, (height - 20.00) / contentHeight);
            }
        }
    }
}
