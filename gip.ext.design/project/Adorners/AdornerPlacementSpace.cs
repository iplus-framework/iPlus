// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace gip.ext.design.Adorners
{
	/// <summary>
	/// Describes the space in which an adorner is placed.
	/// </summary>
	public enum AdornerPlacementSpace
	{
		/// <summary>
		/// The adorner is affected by the render transform of the adorned element.
		/// </summary>
		Render,
		/// <summary>
		/// The adorner is affected by the layout transform of the adorned element.
		/// </summary>
		Layout,
		/// <summary>
		/// The adorner is not affected by transforms of designed controls.
		/// </summary>
		Designer
	}
}
