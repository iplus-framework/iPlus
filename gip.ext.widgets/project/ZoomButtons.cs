﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace gip.ext.widgets
{
	public class ZoomButtons : RangeBase
	{
		static ZoomButtons()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomButtons),
			                                         new FrameworkPropertyMetadata(typeof(ZoomButtons)));
		}
		
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			
			var uxPlus = (ButtonBase)Template.FindName("uxPlus", this);
			var uxMinus = (ButtonBase)Template.FindName("uxMinus", this);
			var uxReset = (ButtonBase)Template.FindName("uxReset", this);
			
			if (uxPlus != null)
				uxPlus.Click += OnZoomInClick;
			if (uxMinus != null)
				uxMinus.Click += OnZoomOutClick;
			if (uxReset != null)
				uxReset.Click += OnResetClick;
		}
		
		const double ZoomFactor = 1.1;
		
		void OnZoomInClick(object sender, EventArgs e)
		{
			SetCurrentValue(ValueProperty, ZoomScrollViewer.RoundToOneIfClose(this.Value * ZoomFactor));
		}
		
		void OnZoomOutClick(object sender, EventArgs e)
		{
			SetCurrentValue(ValueProperty, ZoomScrollViewer.RoundToOneIfClose(this.Value / ZoomFactor));
		}
		
		void OnResetClick(object sender, EventArgs e)
		{
			SetCurrentValue(ValueProperty, 1.0);
		}
	}
}
