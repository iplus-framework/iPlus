// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.design.avui.Extensions;
using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Metadata;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// A thumb where the look can depend on the IsPrimarySelection property.
	/// </summary>
	public class DesignerThumb : Thumb
	{
		/// <summary>
		/// Styled property for <see cref="IsPrimarySelection"/>.
		/// </summary>
		public static readonly StyledProperty<bool> IsPrimarySelectionProperty =
			AvaloniaProperty.Register<DesignerThumb, bool>(nameof(IsPrimarySelection));
		
		/// <summary>
		/// Styled property for <see cref="ThumbVisible"/>.
		/// </summary>
		public static readonly StyledProperty<bool> ThumbVisibleProperty =
			AvaloniaProperty.Register<DesignerThumb, bool>(nameof(ThumbVisible), true);

		/// <summary>
		/// Styled property for <see cref="OperationMenu"/>.
		/// </summary>
		public static readonly StyledProperty<Control[]> OperationMenuProperty =
			AvaloniaProperty.Register<DesignerThumb, Control[]>(nameof(OperationMenu));

		public PlacementAlignment Alignment;
		
		static DesignerThumb()
		{
			// In Avalonia, default styles are typically defined in Themes/Generic.axaml
			// or handled through the styling system
		}

		public void ReDraw()
		{
			var parent = this.TryFindParent<Control>();
			if (parent != null)
				parent.InvalidateArrange();
		}

		/// <summary>
		/// Gets/Sets if the resize thumb is attached to the primary selection.
		/// </summary>
		public bool IsPrimarySelection {
			get { return GetValue(IsPrimarySelectionProperty); }
			set { SetValue(IsPrimarySelectionProperty, value); }
		}
		
		/// <summary>
		/// Gets/Sets if the resize thumb is visible.
		/// </summary>
		public bool ThumbVisible {
			get { return GetValue(ThumbVisibleProperty); }
			set { SetValue(ThumbVisibleProperty, value); }
		}

		/// <summary>
		/// Gets/Sets the OperationMenu.
		/// </summary>
		public Control[] OperationMenu
		{
			get { return GetValue(OperationMenuProperty); }
			set { SetValue(OperationMenuProperty, value); }
		}
	}
}
