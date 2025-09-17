// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using gip.ext.design.avui;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Services;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// A Info text area.
	/// </summary>
	public sealed class InfoTextEnterArea : Control
	{
		Geometry activeAreaGeometry;
		AdornerPanel adornerPanel;
		IDesignPanel designPanel;
		
		public InfoTextEnterArea()
		{
			this.IsHitTestVisible = false;
		}		
			
		public Geometry ActiveAreaGeometry {
			get { return activeAreaGeometry; }
			set {
				activeAreaGeometry = value;
			}
		}	
		
		public static void Start(ref InfoTextEnterArea grayOut, ServiceContainer services, Control activeContainer, string text)
		{
			Debug.Assert(activeContainer != null);
			Start(ref grayOut, services, activeContainer, new Rect(activeContainer.Bounds.Size), text);
		}
		
		public static void Start(ref InfoTextEnterArea grayOut, ServiceContainer services, Control activeContainer, Rect activeRectInActiveContainer, string text)
		{
			Debug.Assert(services != null);
			Debug.Assert(activeContainer != null);
			DesignPanel designPanel = services.GetService<IDesignPanel>() as DesignPanel;
			OptionService optionService = services.GetService<OptionService>();
			if (designPanel != null && grayOut == null && optionService != null && optionService.GrayOutDesignSurfaceExceptParentContainerWhenDragging) {
				grayOut = new InfoTextEnterArea();
				grayOut.designPanel = designPanel;
				grayOut.adornerPanel = new AdornerPanel();
				grayOut.adornerPanel.Order = AdornerOrder.Background;
				grayOut.adornerPanel.SetAdornedElement(designPanel.Context.RootItem.View, null);
				
				// Create RectangleGeometry with transform
				var transform = activeContainer.TransformToVisual(grayOut.adornerPanel.AdornedElement as Visual);
				var rectGeometry = new RectangleGeometry(activeRectInActiveContainer);
				if (transform.HasValue)
				{
					rectGeometry.Transform = new MatrixTransform(transform.Value);
				}
				grayOut.ActiveAreaGeometry = rectGeometry;
				
				var tb = new TextBlock(){Text = text};
				tb.FontSize = 10;
				tb.ClipToBounds = true;
				tb.Width = ((Visual) activeContainer).Bounds.Width;
				tb.Height = ((Visual) activeContainer).Bounds.Height;
				tb.VerticalAlignment = VerticalAlignment.Top;
				tb.HorizontalAlignment = HorizontalAlignment.Left;
				
				// Set transform for TextBlock positioning
				var textTransform = activeContainer.TransformToVisual(grayOut.adornerPanel.AdornedElement as Visual);
				if (textTransform.HasValue)
				{
					tb.RenderTransform = new MatrixTransform(textTransform.Value);
				}
				
				grayOut.adornerPanel.Children.Add(tb);
				
				designPanel.Adorners.Add(grayOut.adornerPanel);
			}
		}
																		 
		public static void Stop(ref InfoTextEnterArea grayOut)
		{
			if (grayOut != null) {
				IDesignPanel designPanel = grayOut.designPanel;
				AdornerPanel adornerPanelToRemove = grayOut.adornerPanel;
				designPanel.Adorners.Remove(adornerPanelToRemove);								
				grayOut = null;
			}
		}
	}
}
