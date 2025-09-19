// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui.Extensions;
using System.Collections.Generic;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia;

namespace gip.ext.designer.avui.Extensions
{
    /// <summary>
    /// The resize thumb around a component.
    /// </summary>
    [ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
    [ExtensionFor(typeof(Control))]
    public sealed class ResizeThumbExtension : SelectionAdornerProvider
    {
        readonly AdornerPanel adornerPanel;
        readonly ResizeThumb[] resizeThumbs;
        /// <summary>An array containing this.ExtendedItem as only element</summary>
        readonly DesignItem[] extendedItemArray = new DesignItem[1];
        IPlacementBehavior resizeBehavior;
        PlacementOperation operation;
        ChangeGroup changeGroup;

        bool _isResizing;

        /// <summary>
        /// Gets whether this extension is resizing any element.
        /// </summary>
        public bool IsResizing
        {
            get { return _isResizing; }
        }

        public ResizeThumbExtension()
        {
            adornerPanel = new AdornerPanel();
            adornerPanel.Order = AdornerOrder.Foreground;
            this.Adorners.Add(adornerPanel);

            resizeThumbs = new ResizeThumb[] {
                CreateThumb(PlacementAlignment.TopLeft, new Cursor(StandardCursorType.SizeAll)),
                CreateThumb(PlacementAlignment.Top, new Cursor(StandardCursorType.SizeNorthSouth)),
                CreateThumb(PlacementAlignment.TopRight, new Cursor(StandardCursorType.SizeAll)),
                CreateThumb(PlacementAlignment.Left, new Cursor(StandardCursorType.SizeWestEast)),
                CreateThumb(PlacementAlignment.Right, new Cursor(StandardCursorType.SizeWestEast)),
                CreateThumb(PlacementAlignment.BottomLeft, new Cursor(StandardCursorType.SizeAll)),
                CreateThumb(PlacementAlignment.Bottom, new Cursor(StandardCursorType.SizeNorthSouth)),
                CreateThumb(PlacementAlignment.BottomRight, new Cursor(StandardCursorType.SizeAll))
            };
        }

        ResizeThumb CreateThumb(PlacementAlignment alignment, Cursor cursor)
        {
            ResizeThumb resizeThumb = new ResizeThumbImpl(cursor.ToString() == StandardCursorType.SizeNorthSouth.ToString(), cursor.ToString() == StandardCursorType.SizeWestEast.ToString());
            resizeThumb.Cursor = cursor;
            resizeThumb.Alignment = alignment;
            AdornerPanel.SetPlacement(resizeThumb, Place(ref resizeThumb, alignment));
            adornerPanel.Children.Add(resizeThumb);

            DragListener drag = new DragListener(resizeThumb);
            drag.Started += new DragHandler(drag_Started);
            drag.Changed += new DragHandler(drag_Changed);
            drag.Completed += new DragHandler(drag_Completed);
            return resizeThumb;
        }

        /// <summary>
        /// Places resize thumbs at their respective positions
        /// and streches out thumbs which are at the center of outline to extend resizability across the whole outline
        /// </summary>
        /// <param name="resizeThumb"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        private RelativePlacement Place(ref ResizeThumb resizeThumb, PlacementAlignment alignment)
        {
            RelativePlacement placement = new RelativePlacement(alignment.Horizontal, alignment.Vertical);

            if (alignment.Horizontal == HorizontalAlignment.Center)
            {
                placement.WidthRelativeToContentWidth = 1;
                placement.HeightOffset = 6;
                resizeThumb.Opacity = 0;
                return placement;
            }
            if (alignment.Vertical == VerticalAlignment.Center)
            {
                placement.HeightRelativeToContentHeight = 1;
                placement.WidthOffset = 6;
                resizeThumb.Opacity = 0;
                return placement;
            }

            placement.WidthOffset = 6;
            placement.HeightOffset = 6;
            return placement;
        }

        Size oldSize;

        // TODO : Remove all hide/show extensions from here.
        void drag_Started(DragListener drag)
        {
            /* Abort editing Text if it was editing, because it interferes with the undo stack. */
            foreach (var extension in this.ExtendedItem.Extensions)
            {
                if (extension is InPlaceEditorExtension)
                {
                    ((InPlaceEditorExtension)extension).AbortEdit();
                }
            }

            oldSize = new Size(ModelTools.GetWidth(ExtendedItem.View), ModelTools.GetHeight(ExtendedItem.View));
            if (resizeBehavior != null)
                operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
            else
            {
                changeGroup = this.ExtendedItem.Context.OpenGroup("Resize", extendedItemArray);
            }
            _isResizing = true;
            ShowSizeAndHideHandles();
        }

        void drag_Changed(DragListener drag)
        {
            double dx = 0;
            double dy = 0;
            var alignment = (drag.Target as ResizeThumb).Alignment;

            if (alignment.Horizontal == HorizontalAlignment.Left) dx = -drag.Delta.X;
            if (alignment.Horizontal == HorizontalAlignment.Right) dx = drag.Delta.X;
            if (alignment.Vertical == VerticalAlignment.Top) dy = -drag.Delta.Y;
            if (alignment.Vertical == VerticalAlignment.Bottom) dy = drag.Delta.Y;

            var newWidth = Math.Max(0, oldSize.Width + dx);
            var newHeight = Math.Max(0, oldSize.Height + dy);

            if (operation.CurrentContainerBehavior is GridPlacementSupport)
            {
                var hor = this.ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].GetConvertedValueOnInstance<HorizontalAlignment>();
                var ver = this.ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].GetConvertedValueOnInstance<VerticalAlignment>();
                if (hor == HorizontalAlignment.Stretch)
                    this.ExtendedItem.Properties[Layoutable.WidthProperty].Reset();
                else
                    this.ExtendedItem.Properties.GetProperty(Layoutable.WidthProperty).SetValue(newWidth);

                if (ver == VerticalAlignment.Stretch)
                    this.ExtendedItem.Properties[Layoutable.HeightProperty].Reset();
                else
                    this.ExtendedItem.Properties.GetProperty(Layoutable.HeightProperty).SetValue(newHeight);
            }
            else
            {
                ModelTools.Resize(ExtendedItem, newWidth, newHeight);
            }

            if (operation != null)
            {
                var info = operation.PlacedItems[0];
                var result = info.OriginalBounds;

                double x = 0, y = 0, width, height;
                if (alignment.Horizontal == HorizontalAlignment.Left)
                    x = Math.Min(result.Right, result.X - dx);
                if (alignment.Vertical == VerticalAlignment.Top)
                    y = Math.Min(result.Bottom, result.Y - dy);
                width = newWidth;
                height = newHeight;
                result = new Rect(x, y, width, height);

                info.Bounds = result.Round();
                info.ResizeThumbAlignment = alignment;
                operation.CurrentContainerBehavior.BeforeSetPosition(operation);
                operation.CurrentContainerBehavior.SetPosition(info);
            }
        }

        void drag_Completed(DragListener drag)
        {
            if (operation != null)
            {
                if (drag.IsCanceled)
                    operation.Abort();
                else
                    operation.Commit();
                operation = null;
            }
            else
            {
                if (drag.IsCanceled)
                    changeGroup.Abort();
                else
                    changeGroup.Commit();
                changeGroup = null;
            }
            _isResizing = false;
            HideSizeAndShowHandles();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            extendedItemArray[0] = this.ExtendedItem;
            this.ExtendedItem.PropertyChanged += OnPropertyChanged;
            this.Services.Selection.PrimarySelectionChanged += OnPrimarySelectionChanged;
            resizeBehavior = PlacementOperation.GetPlacementBehavior(extendedItemArray);
            UpdateAdornerVisibility();
            OnPrimarySelectionChanged(null, null);
        }

        void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateAdornerVisibility();
        }

        protected override void OnRemove()
        {
            this.ExtendedItem.PropertyChanged -= OnPropertyChanged;
            this.Services.Selection.PrimarySelectionChanged -= OnPrimarySelectionChanged;
            base.OnRemove();
        }

        void OnPrimarySelectionChanged(object sender, EventArgs e)
        {
            bool isPrimarySelection = this.Services.Selection.PrimarySelection == this.ExtendedItem;
            foreach (ResizeThumb g in adornerPanel.Children)
            {
                g.IsPrimarySelection = isPrimarySelection;
            }
        }

        void UpdateAdornerVisibility()
        {
            Control fe = this.ExtendedItem.View as Control;
            foreach (ResizeThumb r in resizeThumbs)
            {
                bool isVisible = resizeBehavior != null && resizeBehavior.CanPlace(extendedItemArray, PlacementType.Resize, r.Alignment);
                r.IsVisible = isVisible;
            }
        }

        void ShowSizeAndHideHandles()
        {
            SizeDisplayExtension sizeDisplay = null;
            MarginHandleExtension marginDisplay = null;
            foreach (var extension in ExtendedItem.Extensions)
            {
                if (extension is SizeDisplayExtension)
                    sizeDisplay = extension as SizeDisplayExtension;
                if (extension is MarginHandleExtension)
                    marginDisplay = extension as MarginHandleExtension;
            }

            if (sizeDisplay != null)
            {
                sizeDisplay.HeightDisplay.IsVisible = true;
                sizeDisplay.WidthDisplay.IsVisible = true;
            }

            if (marginDisplay != null)
                marginDisplay.HideHandles();
        }

        void HideSizeAndShowHandles()
        {
            SizeDisplayExtension sizeDisplay = null;
            MarginHandleExtension marginDisplay = null;
            foreach (var extension in ExtendedItem.Extensions)
            {
                if (extension is SizeDisplayExtension)
                    sizeDisplay = extension as SizeDisplayExtension;
                if (extension is MarginHandleExtension)
                    marginDisplay = extension as MarginHandleExtension;
            }

            if (sizeDisplay != null)
            {
                sizeDisplay.HeightDisplay.IsVisible = false;
                sizeDisplay.WidthDisplay.IsVisible = false;
            }
            if (marginDisplay != null)
            {
                marginDisplay.ShowHandles();
            }
        }
    }
}
