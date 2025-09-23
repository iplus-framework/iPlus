// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using gip.ext.design.avui.Extensions;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// The rectangle shown during a rubber-band selecting operation.
	/// </summary>
	public class SelectionFrame : TemplatedControl
	{
		static SelectionFrame()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectionFrame), new FrameworkPropertyMetadata(typeof(SelectionFrame)));
		}
	}
}
