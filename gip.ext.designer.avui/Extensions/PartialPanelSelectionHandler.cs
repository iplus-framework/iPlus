// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Collections.Generic;
using gip.ext.designer.avui.Services;
using gip.ext.design.avui.Extensions;
using gip.ext.design.avui;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia;
using Avalonia.Controls;

namespace gip.ext.designer.avui.Extensions
{
	public class PartialPanelSelectionHandler : BehaviorExtension, IHandlePointerToolMouseDown
	{
		protected override void OnInitialized()
		{
			base.OnInitialized();
			this.ExtendedItem.AddBehavior(typeof(IHandlePointerToolMouseDown), this);
		}
		
		#region IHandlePointerToolMouseDown

		public void HandleSelectionMouseDown(IDesignPanel designPanel, PointerEventArgs e, DesignPanelHitTestResult result)
		{
			if (e.Properties.IsLeftButtonPressed && MouseGestureBase.IsOnlyButtonPressed(e, RawPointerEventType.LeftButtonDown))
			{
				e.Handled = true;
				new PartialRangeSelectionGesture(result.ModelHit).Start(designPanel, e);
			}
		}
		
		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	internal class PartialRangeSelectionGesture : RangeSelectionGesture
	{
		public PartialRangeSelectionGesture(DesignItem container)
			: base(container)
		{
		}

		protected override ICollection<DesignItem> GetChildDesignItemsInContainer(DesignItem container, Geometry geometry, PointerEventArgs e)
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
                if (((GeometryHitTestResult)result).IntersectionDetail == IntersectionDetail.FullyInside || (e.Properties.IsLeftButtonPressed && ((GeometryHitTestResult)result).IntersectionDetail == IntersectionDetail.Intersects))
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

			//HitTestFilterCallback filterCallback = delegate(DependencyObject potentialHitTestTarget)
			//{
			//	FrameworkElement element = potentialHitTestTarget as FrameworkElement;
			//	if (element != null)
			//	{
			//		// ensure we are able to select elements with width/height=0
			//		if (element.ActualWidth == 0 || element.ActualHeight == 0)
			//		{
			//			DependencyObject tmp = element;
			//			DesignItem model = null;
			//			while (tmp != null)
			//			{
			//				model = viewService.GetModel(tmp);
			//				if (model != null) break;
			//				tmp = VisualTreeHelper.GetParent(tmp);
			//			}
			//			if (model != container)
			//			{
			//				resultItems.Add(model);
			//				return HitTestFilterBehavior.ContinueSkipChildren;
			//			}
			//		}
			//	}
			//	return HitTestFilterBehavior.Continue;
			//};

			//HitTestResultCallback resultCallback = delegate(HitTestResult result)
			//{
			//	if (((GeometryHitTestResult)result).IntersectionDetail == IntersectionDetail.FullyInside || (Mouse.RightButton== MouseButtonState.Pressed && ((GeometryHitTestResult)result).IntersectionDetail == IntersectionDetail.Intersects))
			//	{
			//		// find the model for the visual contained in the selection area
			//		DependencyObject tmp = result.VisualHit;
			//		DesignItem model = null;
			//		while (tmp != null)
			//		{
			//			model = viewService.GetModel(tmp);
			//			if (model != null) break;
			//			tmp = VisualTreeHelper.GetParent(tmp);
			//		}
			//		if (model != container)
			//		{
			//			resultItems.Add(model);
			//		}
			//	}
			//	return HitTestResultBehavior.Continue;
			//};

			//VisualTreeHelper.HitTest(container.View, filterCallback, resultCallback, new GeometryHitTestParameters(geometry));
			//return resultItems;
		}
	}
}
