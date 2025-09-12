// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia;

namespace gip.ext.design.avui
{
	/// <summary>
	/// Extension methods used in the WPF designer.
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Rounds position and size of a Rect to PlacementInformation.BoundsPrecision digits. 
		/// </summary>
		public static Rect Round(this Rect rect)
		{
			return new Rect(
				Math.Round(rect.X, PlacementInformation.BoundsPrecision),
				Math.Round(rect.Y, PlacementInformation.BoundsPrecision),
				Math.Round(rect.Width, PlacementInformation.BoundsPrecision),
				Math.Round(rect.Height, PlacementInformation.BoundsPrecision)
			);
		}
	}
}
