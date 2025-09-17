// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls.Primitives;
using Avalonia;
using Avalonia.Styling;

namespace gip.ext.designer.avui.Controls
{
	public class EnumButton : ToggleButton
	{
		public static readonly StyledProperty<object> ValueProperty =
			AvaloniaProperty.Register<EnumButton, object>("Value");

		public object Value {
			get { return GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
	}
}
