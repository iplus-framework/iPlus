// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui.Extensions;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionFor(typeof(FrameworkElement))]
	[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
	public class CanvasPositionExtension : AdornerProvider
	{
		private MarginHandle[] _handles;
		private MarginHandle _leftHandle, _topHandle, _rightHandle, _bottomHandle;
		private Canvas _canvas;

		protected override void OnInitialized()
		{
			base.OnInitialized();
			if (this.ExtendedItem.Parent != null)
			{
				if (this.ExtendedItem.Parent.ComponentType == typeof(Canvas))
				{
					FrameworkElement extendedControl = (FrameworkElement)this.ExtendedItem.Component;
					AdornerPanel adornerPanel = new AdornerPanel();

					// If the Element is rotated/skewed in the grid, then margin handles do not appear
					if (extendedControl.LayoutTransform.Value == Matrix.Identity && extendedControl.RenderTransform.Value == Matrix.Identity)
					{
						_canvas = this.ExtendedItem.Parent.View as Canvas;
						_handles = new[]
						{
							_leftHandle = new CanvasPositionHandle(ExtendedItem, adornerPanel, HandleOrientation.Left),
							_topHandle = new CanvasPositionHandle(ExtendedItem, adornerPanel, HandleOrientation.Top),
							_rightHandle = new CanvasPositionHandle(ExtendedItem, adornerPanel, HandleOrientation.Right),
							_bottomHandle = new CanvasPositionHandle(ExtendedItem, adornerPanel, HandleOrientation.Bottom),
						};
					}

					if (adornerPanel != null)
						this.Adorners.Add(adornerPanel);
				}
			}
		}

		public void HideHandles()
		{
			if (_handles != null)
			{
				foreach (var handle in _handles)
				{
					handle.ShouldBeVisible = false;
					handle.Visibility = Visibility.Hidden;
				}
			}
		}

		public void ShowHandles()
		{
			if (_handles != null)
			{
				foreach (var handle in _handles)
				{
					handle.ShouldBeVisible = true;
					handle.Visibility = Visibility.Visible;
					handle.DecideVisiblity(handle.HandleLength);
				}
			}
		}
	}
}
