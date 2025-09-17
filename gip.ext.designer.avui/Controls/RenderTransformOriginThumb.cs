// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls.Primitives;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// Description of RenderTransformThumb.
	/// </summary>
	public class RenderTransformOriginThumb : Thumb
	{
		static RenderTransformOriginThumb()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(RenderTransformOriginThumb), new FrameworkPropertyMetadata(typeof(RenderTransformOriginThumb)));
		}
	}
}
