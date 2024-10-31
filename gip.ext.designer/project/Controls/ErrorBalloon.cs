// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;

namespace gip.ext.designer.Controls
{
	/// <summary>
	/// An ErrorBalloon window.
	/// </summary>
	public class ErrorBalloon : Window
	{
		static ErrorBalloon()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ErrorBalloon), new FrameworkPropertyMetadata(typeof(ErrorBalloon)));
		}
	}
}
