// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Linq;
using System.Windows;

namespace gip.ext.designer.avui.Controls
{
	public class RotateThumb : DesignerThumb
	{
		static RotateThumb()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(RotateThumb), new FrameworkPropertyMetadata(typeof(RotateThumb)));
		}

		public RotateThumb()
		{
			this.ThumbVisible = true;
		}
	}
}
