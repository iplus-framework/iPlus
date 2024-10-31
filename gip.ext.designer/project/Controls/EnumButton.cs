// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace gip.ext.designer.Controls
{
	public class EnumButton : ToggleButton
	{
		static EnumButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumButton), 
				new FrameworkPropertyMetadata(typeof(EnumButton)));
		}

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(object), typeof(EnumButton));

		public object Value {
			get { return (object)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
	}
}
