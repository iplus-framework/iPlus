// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using gip.ext.design.avui.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// A thumb where the look can depend on the IsPrimarySelection property.
	/// </summary>
	public class DesignerThumb : Thumb
	{
		/// <summary>
		/// Dependency property for <see cref="IsPrimarySelection"/>.
		/// </summary>
		public static readonly DependencyProperty IsPrimarySelectionProperty
			= DependencyProperty.Register("IsPrimarySelection", typeof(bool), typeof(DesignerThumb));
		
		/// <summary>
		/// Dependency property for <see cref="IsPrimarySelection"/>.
		/// </summary>
		public static readonly DependencyProperty ThumbVisibleProperty
			= DependencyProperty.Register("ThumbVisible", typeof(bool), typeof(DesignerThumb), new FrameworkPropertyMetadata(SharedInstances.BoxedTrue));

		/// <summary>
		/// Dependency property for <see cref="OperationMenu"/>.
		/// </summary>
		public static readonly DependencyProperty OperationMenuProperty =
			DependencyProperty.Register("OperationMenu", typeof(Control[]), typeof(DesignerThumb), new PropertyMetadata(null));

		public PlacementAlignment Alignment;
		
		static DesignerThumb()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerThumb), new FrameworkPropertyMetadata(typeof(DesignerThumb)));
		}

		public void ReDraw()
		{
			var parent = this.TryFindParent<FrameworkElement>();
			if (parent != null)
				parent.InvalidateArrange();
		}

		/// <summary>
		/// Gets/Sets if the resize thumb is attached to the primary selection.
		/// </summary>
		public bool IsPrimarySelection {
			get { return (bool)GetValue(IsPrimarySelectionProperty); }
			set { SetValue(IsPrimarySelectionProperty, value); }
		}
		
		/// <summary>
		/// Gets/Sets if the resize thumb is visible.
		/// </summary>
		public bool ThumbVisible {
			get { return (bool)GetValue(ThumbVisibleProperty); }
			set { SetValue(ThumbVisibleProperty, value); }
		}

		/// <summary>
		/// Gets/Sets the OperationMenu.
		/// </summary>
		public Control[] OperationMenu
		{
			get { return (Control[])GetValue(OperationMenuProperty); }
			set { SetValue(OperationMenuProperty, value); }
		}
	}
}
