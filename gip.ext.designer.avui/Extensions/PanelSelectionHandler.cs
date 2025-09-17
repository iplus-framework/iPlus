// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Controls;
using gip.ext.designer.avui.Services;
using gip.ext.design.avui.Extensions;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia;
using Avalonia.Media;

namespace gip.ext.designer.avui.Extensions
{
	/// <summary>
	/// Handles selection multiple controls inside a Panel.
	/// </summary>
	[ExtensionFor(typeof(Panel))]
	public class PanelSelectionHandler : BehaviorExtension, IHandlePointerToolMouseDown
	{
        public PanelSelectionHandler()
            : base()
        {
        }

		protected override void OnInitialized()
		{
			base.OnInitialized();
			this.ExtendedItem.AddBehavior(typeof(IHandlePointerToolMouseDown), this);
		}
		
		public void HandleSelectionMouseDown(IDesignPanel designPanel, PointerEventArgs e, DesignPanelHitTestResult result)
		{
			if (e.Properties.IsLeftButtonPressed && MouseGestureBase.IsOnlyButtonPressed(e, RawPointerEventType.LeftButtonDown)) {
				e.Handled = true;
				new RangeSelectionGesture(result.ModelHit).Start(designPanel, e);
			}
		}
	}

    public class RangeSelectionGesture : ClickOrDragMouseGesture
	{
		protected DesignItem container;
        protected AdornerPanel adornerPanel;
        protected SelectionFrame selectionFrame;

        protected GrayOutDesignerExceptActiveArea grayOut;
		
		public RangeSelectionGesture(DesignItem container)
		{
			this.container = container;
			this.positionRelativeTo = container.View;
		}
		
		protected override void OnDragStarted(PointerEventArgs e)
		{
			adornerPanel = new AdornerPanel();
			adornerPanel.SetAdornedElement(container.View, container);
			
			selectionFrame = new SelectionFrame();
			adornerPanel.Children.Add(selectionFrame);
			
			designPanel.Adorners.Add(adornerPanel);
			
			GrayOutDesignerExceptActiveArea.Start(ref grayOut, services, container.View);
		}
		
		protected override void OnMouseMove(object sender, PointerEventArgs e)
		{
			base.OnMouseMove(sender, e);
			if (_HasDragStarted) {
				SetPlacement(e.GetPosition(positionRelativeTo as Visual));
			}
		}
		
		protected override void OnMouseUp(object sender, PointerReleasedEventArgs e)
		{
			if (_HasDragStarted == false) {
				services.Selection.SetSelectedComponents(new DesignItem [] { container }, SelectionTypes.Auto);
			} else {
				Point endPoint = e.GetPosition(positionRelativeTo as Visual);
				Rect frameRect = new Rect(
					Math.Min(startPoint.X, endPoint.X),
					Math.Min(startPoint.Y, endPoint.Y),
					Math.Abs(startPoint.X - endPoint.X),
					Math.Abs(startPoint.Y - endPoint.Y)
				);
				
				ICollection<DesignItem> items = GetChildDesignItemsInContainer(container, new RectangleGeometry(frameRect), e);
				if (items.Count == 0) {
					items.Add(container);
				}
				services.Selection.SetSelectedComponents(items, SelectionTypes.Auto);
			}
			Stop(e);
		}
		
		protected virtual ICollection<DesignItem> GetChildDesignItemsInContainer(DesignItem container, Geometry geometry, PointerEventArgs e)
		{
            HashSet<DesignItem> resultItems = new HashSet<DesignItem>();
            ViewService viewService = container.Services.View;

            HitTestFilterCallback filterCallback = delegate (Visual potentialHitTestTarget) {
                Control element = potentialHitTestTarget as Control;
                if (element != null)
                {
                    // ensure we are able to select elements with width/height=0
                    if (element.Bounds.Width == 0 || element.Bounds.Height == 0)
                    {
                        Visual tmp = element;
                        DesignItem model = null;
                        while (tmp != null)
                        {
                            model = viewService.GetModel(tmp as AvaloniaObject);
                            if (model != null) break;
                            tmp = VisualTreeHelper.GetParent(tmp) as Visual;
                        }
                        if (model != container)
                        {
                            resultItems.Add(model);
                            return HitTestFilterBehavior.ContinueSkipChildren;
                        }
                    }
                }
                return HitTestFilterBehavior.Continue;
            };

            HitTestResultCallback resultCallback = delegate (HitTestResult result) {
                if (((GeometryHitTestResult)result).IntersectionDetail == IntersectionDetail.FullyInside)
                {
                    // find the model for the visual contained in the selection area
                    Visual tmp = result.VisualHit;
                    DesignItem model = null;
                    while (tmp != null)
                    {
                        model = viewService.GetModel(tmp as AvaloniaObject);
                        if (model != null) break;
                        tmp = VisualTreeHelper.GetParent(tmp) as Visual;
                    }
                    if (model != container)
                    {
                        resultItems.Add(model);
                    }
                }
                return HitTestResultBehavior.Continue;
            };

            VisualTreeHelper.HitTest(container.View as Visual, filterCallback, resultCallback, new GeometryHitTestParameters(geometry));
            return resultItems;

   //         HashSet<DesignItem> resultItems = new HashSet<DesignItem>();
			//ViewService viewService = container.Services.View;
			
			//HitTestFilterCallback filterCallback = delegate(DependencyObject potentialHitTestTarget) {
			//	FrameworkElement element = potentialHitTestTarget as FrameworkElement;
			//	if (element != null) {
			//		// ensure we are able to select elements with width/height=0
			//		if (element.ActualWidth == 0 || element.ActualHeight == 0) {
			//			DependencyObject tmp = element;
			//			DesignItem model = null;
			//			while (tmp != null) {
			//				model = viewService.GetModel(tmp);
			//				if (model != null) break;
			//				tmp = VisualTreeHelper.GetParent(tmp);
			//			}
			//			if (model != container) {
			//				resultItems.Add(model);
			//				return HitTestFilterBehavior.ContinueSkipChildren;
			//			}
			//		}
			//	}
			//	return HitTestFilterBehavior.Continue;
			//};
			
			//HitTestResultCallback resultCallback = delegate(HitTestResult result) {
			//	if (((GeometryHitTestResult) result).IntersectionDetail == IntersectionDetail.FullyInside) {
			//		// find the model for the visual contained in the selection area
			//		DependencyObject tmp = result.VisualHit;
			//		DesignItem model = null;
			//		while (tmp != null) {
			//			model = viewService.GetModel(tmp);
			//			if (model != null) break;
			//			tmp = VisualTreeHelper.GetParent(tmp);
			//		}
			//		if (model != container) {
			//			resultItems.Add(model);
			//		}
			//	}
			//	return HitTestResultBehavior.Continue;
			//};
			
			//VisualTreeHelper.HitTest(container.View, filterCallback, resultCallback, new GeometryHitTestParameters(geometry));
			//return resultItems;
		}
		
		void SetPlacement(Point endPoint)
		{
			RelativePlacement p = new RelativePlacement();
			p.XOffset = Math.Min(startPoint.X, endPoint.X);
			p.YOffset = Math.Min(startPoint.Y, endPoint.Y);
			p.WidthOffset = Math.Max(startPoint.X, endPoint.X) - p.XOffset;
			p.HeightOffset = Math.Max(startPoint.Y, endPoint.Y) - p.YOffset;
			AdornerPanel.SetPlacement(selectionFrame, p);
		}
		
		protected override void OnStopped()
		{
			if (adornerPanel != null) {
				designPanel.Adorners.Remove(adornerPanel);
				adornerPanel = null;
			}
			GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
			selectionFrame = null;
			base.OnStopped();
		}
	}
}
