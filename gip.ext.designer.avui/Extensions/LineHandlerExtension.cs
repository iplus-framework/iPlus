﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.design.avui.Extensions;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia;

namespace gip.ext.designer.avui.Extensions
{
#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
    /// <summary>
    /// Description of LineHandlerExtension.
    /// </summary>
    [ExtensionFor(typeof(Line), OverrideExtensions = new Type[] { typeof(ResizeThumbExtension), typeof(SelectedElementRectangleExtension), typeof(CanvasPositionExtension), typeof(QuickOperationMenuExtension), typeof(RotateThumbExtension), typeof(RenderTransformOriginExtension), typeof(SkewThumbExtension) })]
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
    public class LineHandlerExtension : LineExtensionBase
    {
        private double CurrentX2;
        private double CurrentY2;
        private double CurrentLeft;
        private double CurrentTop;

        //Size oldSize;
        ZoomControl zoom;

        public DragListener DragListener { get; private set; }

        protected DesignerThumb CreateThumb(PlacementAlignment alignment, Cursor cursor)
        {
            DesignerThumb designerThumb = new DesignerThumb { Alignment = alignment, Cursor = cursor, IsPrimarySelection = true };
            AdornerPanel.SetPlacement(designerThumb, Place(designerThumb, alignment));

            adornerPanel.Children.Add(designerThumb);

            DragListener = new DragListener(designerThumb);
            DragListener.Started += drag_Started;
            DragListener.Changed += drag_Changed;
            DragListener.Completed += drag_Completed;

            return designerThumb;
        }

        Bounds CalculateDrawing(double x, double y, double left, double top, double xleft, double xtop)
        {

            Double theta = (180 / Math.PI) * Math.Atan2(y, x);
            double verticaloffset = Math.Abs(90 - Math.Abs(theta));
            if (IsKeyDown(Key.LeftAlt) || IsKeyDown(Key.RightAlt))
            {
                if (Math.Abs(theta) < 45 || Math.Abs(theta) > 135)
                {
                    y = 0;
                    top = xtop;
                }
                else if (verticaloffset < 45)
                {
                    x = 0;
                    left = xleft;
                }
            }
            else if (IsKeyDown(Key.LeftShift) || IsKeyDown(Key.RightShift))
            {
                if (verticaloffset < 10)
                {
                    x = 0;
                    left = xleft;
                }
                else if (Math.Abs(theta) < 10 || Math.Abs(theta) > 170)
                {
                    y = 0;
                    top = xtop;
                }
            }

            SetSurfaceInfo(0, 3, Math.Round((180 / Math.PI) * Math.Atan2(y, x), 0).ToString());
            return new Bounds { X = Math.Round(x, 1), Y = Math.Round(y, 1), Left = Math.Round(left, 1), Top = Math.Round(top, 1) };
        }

        #region eventhandlers


        // TODO : Remove all hide/show extensions from here.
        protected virtual void drag_Started(DragListener drag)
        {
            Line al = ExtendedItem.View as Line;
            CurrentX2 = al.EndPoint.X;
            CurrentY2 = al.EndPoint.X;
            CurrentLeft = al.Margin.Left;
            CurrentTop = al.Margin.Top;
            if (ExtendedItem.Parent.View is Canvas)
            {
                var lp = (double)al.GetValue(Canvas.LeftProperty);
                if (!double.IsNaN(lp))
                    CurrentLeft += lp;
                var tp = (double)al.GetValue(Canvas.TopProperty);
                if (!double.IsNaN(tp))
                    CurrentTop += tp;
            }

            var designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
            zoom = designPanel.TryFindParent<ZoomControl>();

            if (resizeBehavior != null)
                operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
            else
            {
                changeGroup = this.ExtendedItem.Context.OpenGroup("Resize", extendedItemArray);
            }
            _isResizing = true;

            (drag.Target as DesignerThumb).IsPrimarySelection = false;
        }

        protected virtual void drag_Changed(DragListener drag)
        {
            Line al = ExtendedItem.View as Line;

            var alignment = (drag.Target as DesignerThumb).Alignment;
            var info = operation.PlacedItems[0];
            double dx = 0;
            double dy = 0;

            if (zoom != null)
            {
                dx = drag.Delta.X * (1 / zoom.CurrentZoom);
                dy = drag.Delta.Y * (1 / zoom.CurrentZoom);
            }

            double top, left, x, y, xtop, xleft;

            if (alignment == PlacementAlignment.TopLeft)
            {

                //normal values
                x = CurrentX2 - dx;
                y = CurrentY2 - dy;
                top = CurrentTop + dy;
                left = CurrentLeft + dx;

                //values to use when keys are pressed
                xtop = CurrentTop + CurrentY2;
                xleft = CurrentLeft + CurrentX2;

            }
            else
            {
                x = CurrentX2 + dx;
                y = CurrentY2 + dy;
                top = xtop = CurrentTop;
                left = xleft = CurrentLeft;
            }

            Bounds position = CalculateDrawing(x, y, left, top, xleft, xtop);
            ExtendedItem.Properties.GetProperty(Line.StartPointProperty).SetValue(new Point(0, 0));
            ExtendedItem.Properties.GetProperty(Line.EndPointProperty).SetValue(new Point(position.X, position.Y));

            if (operation != null)
            {
                var result = info.OriginalBounds;
                result = new Rect(position.Left, position.Top, Math.Abs(position.X), Math.Abs(position.Y));
                info.Bounds = result.Round();
                operation.CurrentContainerBehavior.BeforeSetPosition(operation);
                operation.CurrentContainerBehavior.SetPosition(info);

                //				var p = operation.CurrentContainerBehavior.PlacePoint(new Point(position.X, position.Y));
                //				ExtendedItem.Properties.GetProperty(Line.X2Property).SetValue(p.X);
                //				ExtendedItem.Properties.GetProperty(Line.Y2Property).SetValue(p.Y);
            }

            (drag.Target as DesignerThumb).InvalidateArrange();
            ResetWidthHeightProperties();
        }

        protected virtual void drag_Completed(DragListener drag)
        {
            if (operation != null)
            {
                if (drag.IsCanceled) operation.Abort();
                else
                {
                    ResetWidthHeightProperties();

                    operation.Commit();
                }
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
            (drag.Target as DesignerThumb).IsPrimarySelection = true;
            HideSizeAndShowHandles();
        }

        /// <summary>
        /// is invoked whenever a line is selected on the canvas, remember that the adorners are created for each line object and never destroyed
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            resizeThumbs = new DesignerThumb[]
            {
                CreateThumb(PlacementAlignment.TopLeft, new Cursor(StandardCursorType.Cross)),
                CreateThumb(PlacementAlignment.BottomRight, new Cursor(StandardCursorType.Cross))
            };

            extendedItemArray[0] = this.ExtendedItem;

            Invalidate();

            this.ExtendedItem.PropertyChanged += OnPropertyChanged;
            resizeBehavior = PlacementOperation.GetPlacementBehavior(extendedItemArray);
            UpdateAdornerVisibility();
        }

        #endregion
    }
}
