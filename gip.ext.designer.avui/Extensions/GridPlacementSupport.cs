// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Layout;

namespace gip.ext.designer.avui.Extensions
{
	/// <summary>
	/// Provides <see cref="IPlacementBehavior"/> behavior for <see cref="Grid"/>.
	/// </summary>
	[ExtensionFor(typeof(Grid), OverrideExtension=typeof(DefaultPlacementBehavior))]
    [CLSCompliant(false)]
    public sealed class GridPlacementSupport : SnaplinePlacementBehavior
	{
		Grid grid;
		private bool enteredIntoNewContainer;

        public static bool NoMarginSuggestion = true;
        public static bool NoSizeSuggestion = true;
        public static bool NoAlignmentSuggestion = true;
		
		protected override void OnInitialized()
		{
			base.OnInitialized();
			grid = (Grid)this.ExtendedItem.Component;
		}
		
		double GetColumnOffset(int index)
		{
			// when the grid has no columns, we still need to return 0 for index=0 and grid.Width for index=1
			if (index == 0)
				return 0;
			else if (index < grid.ColumnDefinitions.Count)
				return 0.0; // grid.ColumnDefinitions[index].Offset;
			else
				return grid.Bounds.Width;
		}
		
		double GetRowOffset(int index)
		{
			if (index == 0)
				return 0;
			else if (index < grid.RowDefinitions.Count)
				return 0.0; // grid.RowDefinitions[index].Offset;
			else
				return grid.Bounds.Height;
		}
		
		const double epsilon = 0.00000001;
		
		int GetColumnIndex(double x)
		{
			if (grid.ColumnDefinitions.Count == 0)
				return 0;
			for (int i = 1; i < grid.ColumnDefinitions.Count; i++) {
				if (x < epsilon) //grid.ColumnDefinitions[i].Offset - epsilon)
					return i - 1;
			}
			return grid.ColumnDefinitions.Count - 1;
		}
		
		int GetRowIndex(double y)
		{
			if (grid.RowDefinitions.Count == 0)
				return 0;
			for (int i = 1; i < grid.RowDefinitions.Count; i++) {
				if (y < epsilon) //grid.RowDefinitions[i].Offset - epsilon)
					return i - 1;
			}
			return grid.RowDefinitions.Count - 1;
		}
		
		int GetEndColumnIndex(double x)
		{
			if (grid.ColumnDefinitions.Count == 0)
				return 0;
			for (int i = 1; i < grid.ColumnDefinitions.Count; i++) {
				if (x <= epsilon) //grid.ColumnDefinitions[i].Offset + epsilon)
					return i - 1;
			}
			return grid.ColumnDefinitions.Count - 1;
		}
		
		int GetEndRowIndex(double y)
		{
			if (grid.RowDefinitions.Count == 0)
				return 0;
			for (int i = 1; i < grid.RowDefinitions.Count; i++) {
				if (y <= epsilon) //grid.RowDefinitions[i].Offset + epsilon)
					return i - 1;
			}
			return grid.RowDefinitions.Count - 1;
		}

        protected override void AddContainerSnaplines(Rect containerRect, List<SnaplinePlacementBehavior.Snapline> horizontalMap, List<SnaplinePlacementBehavior.Snapline> verticalMap)
        {
            var grid = (Grid)ExtendedItem.View;
            double offset = 0;
            foreach (RowDefinition r in grid.RowDefinitions)
            {
                offset += r.ActualHeight;
                horizontalMap.Add(new Snapline() { RequireOverlap = false, Offset = offset, Start = offset, End = containerRect.Right });
                if (SnaplineMargin > 0)
                {
                    horizontalMap.Add(new Snapline() { RequireOverlap = false, Offset = offset - SnaplineMargin, Start = offset, End = containerRect.Right });
                    horizontalMap.Add(new Snapline() { RequireOverlap = false, Offset = offset + SnaplineMargin, Start = offset, End = containerRect.Right });
                }

            }
            offset = 0;
            foreach (ColumnDefinition c in grid.ColumnDefinitions)
            {
                offset += c.ActualWidth;
                verticalMap.Add(new Snapline() { RequireOverlap = false, Offset = offset, Start = containerRect.Top, End = containerRect.Bottom });
                if (SnaplineMargin > 0)
                {
                    verticalMap.Add(new Snapline() { RequireOverlap = false, Offset = offset - SnaplineMargin, Start = containerRect.Top, End = containerRect.Bottom });
                    verticalMap.Add(new Snapline() { RequireOverlap = false, Offset = offset + SnaplineMargin, Start = containerRect.Top, End = containerRect.Bottom });
                }
            }
        }

        static void SetColumn(DesignItem item, int column, int columnSpan)
		{
			Debug.Assert(item != null && column >= 0 && columnSpan > 0);
			item.Properties.GetAttachedProperty(Grid.ColumnProperty).SetValue(column);
			if (columnSpan == 1) {
				item.Properties.GetAttachedProperty(Grid.ColumnSpanProperty).Reset();
			} else {
				item.Properties.GetAttachedProperty(Grid.ColumnSpanProperty).SetValue(columnSpan);
			}
		}
		
		static void SetRow(DesignItem item, int row, int rowSpan)
		{
			Debug.Assert(item != null && row >= 0 && rowSpan > 0);
			item.Properties.GetAttachedProperty(Grid.RowProperty).SetValue(row);
			if (rowSpan == 1) {
				item.Properties.GetAttachedProperty(Grid.RowSpanProperty).Reset();
			} else {
				item.Properties.GetAttachedProperty(Grid.RowSpanProperty).SetValue(rowSpan);
			}
		}
		
		static HorizontalAlignment SuggestHorizontalAlignment(Rect itemBounds, Rect availableSpaceRect)
		{
			bool isLeft = itemBounds.Left < availableSpaceRect.Left + availableSpaceRect.Width / 4;
			bool isRight = itemBounds.Right > availableSpaceRect.Right - availableSpaceRect.Width / 4;
			if (isLeft && isRight)
				return HorizontalAlignment.Stretch;
			else if (isRight)
				return HorizontalAlignment.Right;
			else
				return HorizontalAlignment.Left;
		}
		
		static VerticalAlignment SuggestVerticalAlignment(Rect itemBounds, Rect availableSpaceRect)
		{
			bool isTop = itemBounds.Top < availableSpaceRect.Top + availableSpaceRect.Height / 4;
			bool isBottom = itemBounds.Bottom > availableSpaceRect.Bottom - availableSpaceRect.Height / 4;
			if (isTop && isBottom)
				return VerticalAlignment.Stretch;
			else if (isBottom)
				return VerticalAlignment.Bottom;
			else
				return VerticalAlignment.Top;
		}
		
		public override void EnterContainer(PlacementOperation operation)
		{
            enteredIntoNewContainer = true;
            grid.UpdateLayout();
            base.EnterContainer(operation);

            if (operation.Type == PlacementType.PasteItem)
            {
                foreach (PlacementInformation info in operation.PlacedItems)
                {
                    var margin = info.Item.Properties.GetProperty(Control.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                    var horizontalAlignment = info.Item.Properties.GetProperty(Control.HorizontalAlignmentProperty).GetConvertedValueOnInstance<HorizontalAlignment>();
                    var verticalAlignment = info.Item.Properties.GetProperty(Control.VerticalAlignmentProperty).GetConvertedValueOnInstance<VerticalAlignment>();

					double left = margin.Left;
                    double right = margin.Right;
					double top = margin.Top;
					double bottom = margin.Bottom;

                    if (horizontalAlignment == HorizontalAlignment.Left)
                        left += PlacementOperation.PasteOffset;
                    else if (horizontalAlignment == HorizontalAlignment.Right)
                        right -= PlacementOperation.PasteOffset;

                    if (verticalAlignment == VerticalAlignment.Top)
                        top += PlacementOperation.PasteOffset;
                    else if (verticalAlignment == VerticalAlignment.Bottom)
                        bottom -= PlacementOperation.PasteOffset;

                    margin = new Thickness(left, top, right, bottom);
                    info.Item.Properties.GetProperty(Control.MarginProperty).SetValue(margin);
                }
            }
        }
		
		GrayOutDesignerExceptActiveArea grayOut;
		
		public override void EndPlacement(PlacementOperation operation)
		{
			GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
			enteredIntoNewContainer=false;
			base.EndPlacement(operation);
		}
		
		public override void SetPosition(PlacementInformation info)
		{
			base.SetPosition(info);
			int leftColumnIndex = GetColumnIndex(info.Bounds.Left);
			int rightColumnIndex = GetEndColumnIndex(info.Bounds.Right);
			if (rightColumnIndex < leftColumnIndex) rightColumnIndex = leftColumnIndex;
			SetColumn(info.Item, leftColumnIndex, rightColumnIndex - leftColumnIndex + 1);
			int topRowIndex = GetRowIndex(info.Bounds.Top);
			int bottomRowIndex = GetEndRowIndex(info.Bounds.Bottom);
			if (bottomRowIndex < topRowIndex) bottomRowIndex = topRowIndex;
			SetRow(info.Item, topRowIndex, bottomRowIndex - topRowIndex + 1);
			
			Rect availableSpaceRect = new Rect(
				new Point(GetColumnOffset(leftColumnIndex), GetRowOffset(topRowIndex)),
				new Point(GetColumnOffset(rightColumnIndex + 1), GetRowOffset(bottomRowIndex + 1))
			);
			if (info.Item == Services.Selection.PrimarySelection) {
				// only for primary selection:
				if (grayOut != null) {
					grayOut.AnimateActiveAreaRectTo(availableSpaceRect);
				} else {
					GrayOutDesignerExceptActiveArea.Start(ref grayOut, this.Services, this.ExtendedItem.View, availableSpaceRect);
				}
			}
			
			HorizontalAlignment ha = (HorizontalAlignment)info.Item.Properties[Control.HorizontalAlignmentProperty].ValueOnInstance;
			VerticalAlignment va = (VerticalAlignment)info.Item.Properties[Control.VerticalAlignmentProperty].ValueOnInstance;
			if(enteredIntoNewContainer){
				ha = SuggestHorizontalAlignment(info.Bounds, availableSpaceRect);
				va = SuggestVerticalAlignment(info.Bounds, availableSpaceRect);
			}
            if (!NoAlignmentSuggestion)
            {
                info.Item.Properties[Control.HorizontalAlignmentProperty].SetValue(ha);
                info.Item.Properties[Control.VerticalAlignmentProperty].SetValue(va);
            }

            if (!NoMarginSuggestion)
            {
                double left = 0;
                double right = 0;
                double top = 0;
                double bottom = 0;

                if (ha == HorizontalAlignment.Left || ha == HorizontalAlignment.Stretch)
                    left = info.Bounds.Left - GetColumnOffset(leftColumnIndex);
                if (va == VerticalAlignment.Top || va == VerticalAlignment.Stretch)
                    top = info.Bounds.Top - GetRowOffset(topRowIndex);
                if (ha == HorizontalAlignment.Right || ha == HorizontalAlignment.Stretch)
                    right = GetColumnOffset(rightColumnIndex + 1) - info.Bounds.Right;
                if (va == VerticalAlignment.Bottom || va == VerticalAlignment.Stretch)
                    bottom = GetRowOffset(bottomRowIndex + 1) - info.Bounds.Bottom;
                Thickness margin = new Thickness(left, top, right, bottom);
                info.Item.Properties[Control.MarginProperty].SetValue(margin);
            }

            if (!NoSizeSuggestion)
            {
                var widthIsSet = info.Item.Properties[Layoutable.WidthProperty].IsSet;
                var heightIsSet = info.Item.Properties[Layoutable.HeightProperty].IsSet;
                if (!widthIsSet)
                {
                    if (ha == HorizontalAlignment.Stretch)
                        info.Item.Properties[Layoutable.WidthProperty].Reset();
                    else
                        info.Item.Properties[Layoutable.WidthProperty].SetValue(info.Bounds.Width);
                }
                else
                {
                    info.Item.Properties[Layoutable.WidthProperty].SetValue(info.Bounds.Width);
                }
                if (!heightIsSet)
                {
                    if (va == VerticalAlignment.Stretch)
                        info.Item.Properties[Layoutable.HeightProperty].Reset();
                    else
                        info.Item.Properties[Layoutable.HeightProperty].SetValue(info.Bounds.Height);
                }
                else
                {
                    info.Item.Properties[Layoutable.HeightProperty].SetValue(info.Bounds.Height);
                }
            }
		}
		
		public override void LeaveContainer(PlacementOperation operation)
		{
			GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
			base.LeaveContainer(operation);
			foreach (PlacementInformation info in operation.PlacedItems) {
				if (info.Item.ComponentType == typeof(ColumnDefinition)) {
					// TODO: combine the width of the deleted column with the previous column
					this.ExtendedItem.Properties["ColumnDefinitions"].CollectionElements.Remove(info.Item);
				} else if (info.Item.ComponentType == typeof(RowDefinition)) {
					this.ExtendedItem.Properties["RowDefinitions"].CollectionElements.Remove(info.Item);
				} else {
					info.Item.Properties.GetAttachedProperty(Grid.RowProperty).Reset();
					info.Item.Properties.GetAttachedProperty(Grid.ColumnProperty).Reset();
					info.Item.Properties.GetAttachedProperty(Grid.RowSpanProperty).Reset();
					info.Item.Properties.GetAttachedProperty(Grid.ColumnSpanProperty).Reset();

					HorizontalAlignment ha = (HorizontalAlignment)info.Item.Properties[Layoutable.HorizontalAlignmentProperty].ValueOnInstance;
					VerticalAlignment va = (VerticalAlignment)info.Item.Properties[Layoutable.VerticalAlignmentProperty].ValueOnInstance;

					if (ha == HorizontalAlignment.Stretch)
						info.Item.Properties[Control.WidthProperty].SetValue(info.Bounds.Width);
					if (va == VerticalAlignment.Stretch)
						info.Item.Properties[Control.HeightProperty].SetValue(info.Bounds.Height);
				}
			}
		}
	}
}
