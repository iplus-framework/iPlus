﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using gip.ext.design.Adorners;
using gip.ext.design.Extensions;
using gip.ext.design;

namespace gip.ext.designer.Extensions
{
	/// <summary>
	/// Provides <see cref="IPlacementBehavior"/> for <see cref="StackPanel"/>.
	/// </summary>
	[ExtensionFor(typeof (StackPanel), OverrideExtension = typeof (DefaultPlacementBehavior))]
    [CLSCompliant(false)]
    public class StackPanelPlacementSupport : DefaultPlacementBehavior
    {
        private StackPanel _stackPanel;
        private AdornerPanel _adornerPanel;
        private Rectangle _rectangle = new Rectangle();        // Draws a rectangle to indicate the position of insertion. 
        private readonly List<Rect> _rects = new List<Rect>(); // Contains the Rect of all the children of StackPanel.
        
        
        private bool _isItemGettingResized;                    // Indicates whether any children is getting resized.
        private int _indexToInsert;                            // Postion where to insert the element.


        protected override void OnInitialized()
        {
            base.OnInitialized();
            _stackPanel = this.ExtendedItem.View as StackPanel;
            var children = this.ExtendedItem.ContentProperty.CollectionElements;
            foreach (var child in children) {
                Point p = child.View.TranslatePoint(new Point(0, 0), this.ExtendedItem.View);
                _rects.Add(new Rect(p, child.View.RenderSize));
            }
        }

        public override void BeginPlacement(PlacementOperation operation)
        {
            base.BeginPlacement(operation);
            if (_rects.Count > 0)
                _rects.Clear();
            
            /* Add Rect of all children to _rects */
            var children = this.ExtendedItem.ContentProperty.CollectionElements;
            foreach (var child in children) {
                Point p = child.View.TranslatePoint(new Point(0, 0), this.ExtendedItem.View);
                _rects.Add(new Rect(p, child.View.RenderSize));
            }
            if (_adornerPanel != null && this.ExtendedItem.Services.DesignPanel.Adorners.Contains(_adornerPanel))
                this.ExtendedItem.Services.DesignPanel.Adorners.Remove(_adornerPanel);
            
            /* Place the Rectangle */
            _adornerPanel = new AdornerPanel();
            _rectangle = new Rectangle();
            _adornerPanel.SetAdornedElement(this.ExtendedItem.View, this.ExtendedItem);
            _adornerPanel.Children.Add(_rectangle);
            this.ExtendedItem.Services.DesignPanel.Adorners.Add(_adornerPanel);
        }

        public override void EndPlacement(PlacementOperation operation)
        {
            base.EndPlacement(operation);
            if (_adornerPanel != null && this.ExtendedItem.Services.DesignPanel.Adorners.Contains(_adornerPanel))
                this.ExtendedItem.Services.DesignPanel.Adorners.Remove(_adornerPanel);
        }

        public override void EnterContainer(PlacementOperation operation)
        {
            base.EnterContainer(operation);
            foreach (var info in operation.PlacedItems) {
                info.Item.Properties[FrameworkElement.MarginProperty].Reset();
                info.Item.Properties[FrameworkElement.HorizontalAlignmentProperty].Reset();
                info.Item.Properties[FrameworkElement.VerticalAlignmentProperty].Reset();
            }
            _rectangle.Visibility = Visibility.Visible;
        }

        public override void LeaveContainer(PlacementOperation operation)
        {
            base.LeaveContainer(operation);
            /* Hide the rectangle in case switching to the other container
 			   *  otherwise it will show up intersecting with the container */
            _rectangle.Visibility = Visibility.Hidden; 
        }

        public override void SetPosition(PlacementInformation info)
        {
            base.SetPosition(info);

            var resizeExtensions = info.Item.Extensions.OfType<ResizeThumbExtension>();
            if (resizeExtensions != null && resizeExtensions.Count() != 0) {
                var resizeExtension = resizeExtensions.First();
                _isItemGettingResized = resizeExtension.IsResizing;
            }

            if (_stackPanel != null && !_isItemGettingResized) {
                if (_stackPanel.Orientation == Orientation.Vertical) {
                    var offset = FindHorizontalRectanglePlacementOffset(info.Bounds);
                    DrawHorizontalRectangle(offset);
                } else {
                    var offset = FindVerticalRectanglePlacementOffset(info.Bounds);
                    DrawVerticalRectangle(offset);
                }

                ChangePostionTo(info.Item.View, _indexToInsert);
            }
        }

        private void ChangePostionTo(UIElement element, int index)
        {
            int elementIndex = 0;
            if (_stackPanel.Children.Contains(element))
                elementIndex = _stackPanel.Children.IndexOf(element);
            if (index > elementIndex)
                index--;
            _stackPanel.Children.Remove(element);
            _stackPanel.Children.Insert(index, element);
        }
        
        private double FindHorizontalRectanglePlacementOffset(Rect rect)
        {
            _rects.Sort((r1, r2) => r1.Top.CompareTo(r2.Top));
            var itemCenter = (rect.Top + rect.Bottom)/2;
            for (int i = 0; i < _rects.Count; i++) {
                var rectCenter = (_rects[i].Top + _rects[i].Bottom)/2;
                if (rectCenter >= itemCenter) {
                    _indexToInsert = i;
                    return _rects[i].Top;
                }
            }
            _indexToInsert = _rects.Count;
            return _rects.Count > 0 ? _rects.Last().Bottom : 0;
        }
        
        private double FindVerticalRectanglePlacementOffset(Rect rect)
        {
            _rects.Sort((r1, r2) => r1.Left.CompareTo(r2.Left));
            var itemCenter = (rect.Left + rect.Right)/2;
            for (int i = 0; i < _rects.Count; i++) {
                var rectCenter = (_rects[i].Left + _rects[i].Top)/2;
                if (rectCenter >= itemCenter) {
                    _indexToInsert = i;
                    return _rects[i].Left;
                }
            }
            _indexToInsert = _rects.Count;
            return _rects.Count > 0 ? _rects.Last().Right : 0;
        }
        
        private void DrawHorizontalRectangle(double offset)
        {
            _rectangle.Height = 2;
            _rectangle.Fill = Brushes.Black;
            var placement = new RelativePlacement(HorizontalAlignment.Stretch, VerticalAlignment.Top) {YOffset = offset};
            AdornerPanel.SetPlacement(_rectangle, placement);
        }
        
        private void DrawVerticalRectangle(double offset)
        {
            _rectangle.Width = 2;
            _rectangle.Fill = Brushes.Black;
            var placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Stretch) {XOffset = offset};
            AdornerPanel.SetPlacement(_rectangle, placement);
        }
    }
}
