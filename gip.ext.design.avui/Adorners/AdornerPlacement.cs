// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace gip.ext.design.avui.Adorners
{
	/// <summary>
	/// Defines how a design-time adorner is placed.
	/// </summary>
	public abstract class AdornerPlacement
	{
		/// <summary>
		/// A placement instance that places the adorner above the content, using the same bounds as the content.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly AdornerPlacement FillContent = new FillContentPlacement();
		
		/// <summary>
		/// Arranges the adorner element on the specified adorner panel.
		/// </summary>
		public abstract void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize);
		
		sealed class FillContentPlacement : AdornerPlacement
		{
			public override void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize)
			{
				adorner.Arrange(new Rect(adornedElementSize));
			}
		}
	}
}
