// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.avui.Extensions;
using System.ComponentModel;
using gip.ext.design.avui.Adorners;
using System.Diagnostics;
using gip.ext.design.avui;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using gip.ext.designer.avui.Services;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia.Collections;
using Avalonia.Layout;
using gip.ext.design.avui.UIExtensions;

namespace gip.ext.designer.avui.Extensions
{
    public class SnaplinePlacementBehavior : DefaultPlacementBehavior
    {
        public static bool GetDisableSnaplines(AvaloniaObject obj)
        {
            return (bool)obj.GetValue(DisableSnaplinesProperty);
        }

        public static void SetDisableSnaplines(AvaloniaObject obj, bool value)
        {
            obj.SetValue(DisableSnaplinesProperty, value);
        }

        public static readonly DependencyProperty DisableSnaplinesProperty =
            DependencyProperty.RegisterAttached("DisableSnaplines", typeof(bool), typeof(SnaplinePlacementBehavior), new PropertyMetadata(false));

        AdornerPanel adornerPanel;
        Canvas surface;
        List<Snapline> horizontalMap;
        List<Snapline> verticalMap;
        double? baseline;

        private static double _snaplineMargin = 8;
        public static double SnaplineMargin
        {
            get { return _snaplineMargin; }
            set { _snaplineMargin = value; }
        }

        private static double _snaplineAccuracy = 5;
        public static double SnaplineAccuracy
        {
            get { return _snaplineAccuracy; }
            set { _snaplineAccuracy = value; }
        }

        public override void BeginPlacement(PlacementOperation operation)
        {
            base.BeginPlacement(operation);
            CreateSurface(operation);
        }

        public override void EndPlacement(PlacementOperation operation)
        {
            base.EndPlacement(operation);
            DeleteSurface();
        }

        public override void EnterContainer(PlacementOperation operation)
        {
            base.EnterContainer(operation);
            CreateSurface(operation);
        }

        public override void LeaveContainer(PlacementOperation operation)
        {
            base.LeaveContainer(operation);
            DeleteSurface();
        }

        public override Point PlacePoint(Point point)
        {
            if (surface == null)
                return base.PlacePoint(point);

            DesignPanel designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
            if (designPanel == null || !designPanel.UseSnaplinePlacement)
                return base.PlacePoint(point); ;

            surface.Children.Clear();
            if (IsKeyDown(Key.LeftCtrl))
                return base.PlacePoint(point); ;

            Rect bounds = new Rect(point.X, point.Y, 0, 0);

            var horizontalInput = new List<Snapline>();
            var verticalInput = new List<Snapline>();

            AddLines(bounds, 0, false, horizontalInput, verticalInput, null);
            if (baseline.HasValue)
            {
                var textOffset = bounds.Top + baseline.Value;
                horizontalInput.Add(new Snapline() { Group = 1, Offset = textOffset, Start = bounds.Left, End = bounds.Right });
            }

            List<Snapline> drawLines;
            double delta;

            var newPoint = base.PlacePoint(point);
            double x = newPoint.X, y = newPoint.Y;
            if (Snap(horizontalInput, horizontalMap, SnaplineAccuracy, out drawLines, out delta))
            {
                foreach (var d in drawLines)
                {
                    DrawLine(d.Start, d.Offset + d.DrawOffset, d.End, d.Offset + d.DrawOffset);
                }

                y += delta;
            }
            else
                y = newPoint.Y;

            if (Snap(verticalInput, verticalMap, SnaplineAccuracy, out drawLines, out delta))
            {
                foreach (var d in drawLines)
                {
                    DrawLine(d.Offset + d.DrawOffset, d.Start, d.Offset + d.DrawOffset, d.End);
                }

                x += delta;
            }
            else
                x = newPoint.X;

            point = new Point(x, y);
            return point;
        }

        public override void BeforeSetPosition(PlacementOperation operation)
        {
            base.BeforeSetPosition(operation);
            if (surface == null) return;

            surface.Children.Clear();
            if (IsKeyDown(Key.LeftCtrl)) return;

            Rect bounds = RectExtensions.Empty();
            foreach (var item in operation.PlacedItems)
            {
                bounds.Union(item.Bounds);
            }

            var horizontalInput = new List<Snapline>();
            var verticalInput = new List<Snapline>();
            var info = operation.PlacedItems[0];

            if (operation.Type == PlacementType.Resize)
            {
                AddLines(bounds, 0, false, horizontalInput, verticalInput, info.ResizeThumbAlignment);
            }
            else
            {
                AddLines(bounds, 0, false, horizontalInput, verticalInput, null);
                if (baseline.HasValue)
                {
                    var textOffset = bounds.Top + baseline.Value;
                    horizontalInput.Add(new Snapline() { Group = 1, Offset = textOffset, Start = bounds.Left, End = bounds.Right });
                }
            }

            // debug
            //foreach (var t in horizontalMap.Concat(horizontalInput)) {
            //    surface.Children.Add(new Line() { X1 = t.Start, X2 = t.End, Y1 = t.Offset, Y2 = t.Offset, Stroke = Brushes.Black });
            //}
            //foreach (var t in verticalMap.Concat(verticalInput)) {
            //    surface.Children.Add(new Line() { X1 = t.Offset, X2 = t.Offset, Y1 = t.Start , Y2 = t.End, Stroke = Brushes.Black });
            //}
            //return;

            List<Snapline> drawLines;
            double delta;
            double y = bounds.Y;
            double x = bounds.X;
            double width = bounds.Width;
            double height = bounds.Height;

            if (Snap(horizontalInput, horizontalMap, SnaplineAccuracy, out drawLines, out delta))
            {

                if (operation.Type == PlacementType.Resize)
                {
                    if (info.ResizeThumbAlignment.Vertical == VerticalAlignment.Top)
                    {
                        y += delta;
                        height = Math.Max(0, height - delta);
                        if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        {
                            y = Math.Round(y);
                            height = Math.Round(height);
                        }
                    }
                    else
                    {
                        height = Math.Max(0, height + delta);

                        if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        {
                            height = Math.Round(height);
                        }
                    }
                    info.Bounds = new Rect(x, y, width, height);
                }
                else
                {
                    foreach (var item in operation.PlacedItems)
                    {
                        var r = item.Bounds;
                        double rY = r.Y;
                        rY += delta;
                        if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        {
                            rY = Math.Round(rY);
                        }
                        item.Bounds = new Rect(item.Bounds.X, rY, item.Bounds.Width, item.Bounds.Height);
                    }
                }

                foreach (var d in drawLines)
                {
                    DrawLine(d.Start, d.Offset + d.DrawOffset, d.End, d.Offset + d.DrawOffset);
                }
            }

            if (Snap(verticalInput, verticalMap, SnaplineAccuracy, out drawLines, out delta))
            {

                if (operation.Type == PlacementType.Resize)
                {
                    if (info.ResizeThumbAlignment.Horizontal == HorizontalAlignment.Left)
                    {
                        x += delta;
                        width = Math.Max(0, width - delta);
                        if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        {
                            x = Math.Round(x);
                            width = Math.Round(width);
                        }
                    }
                    else
                    {
                        width = Math.Max(0, width + delta);
                        if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        {
                            width = Math.Round(width);
                        }
                    }
                    info.Bounds = new Rect(x, y, width, height);
                }
                else
                {
                    foreach (var item in operation.PlacedItems)
                    {
                        var r = item.Bounds;
                        double rX = r.X;
                        rX += delta;
                        if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        {
                            rX = Math.Round(rX);
                        }
                        item.Bounds = new Rect(rX, item.Bounds.Y, item.Bounds.Width, item.Bounds.Height);
                        item.Bounds = r;
                    }
                }

                foreach (var d in drawLines)
                {
                    DrawLine(d.Offset + d.DrawOffset, d.Start, d.Offset + d.DrawOffset, d.End);
                }
            }

        }

        void CreateSurface(PlacementOperation operation)
        {
            if (ExtendedItem.Services.GetService<IDesignPanel>() != null)
            {

                surface = new Canvas();
                adornerPanel = new AdornerPanel();
                adornerPanel.SetAdornedElement(ExtendedItem.View, ExtendedItem);
                AdornerPanel.SetPlacement(surface, AdornerPlacement.FillContent);
                adornerPanel.Children.Add(surface);
                ExtendedItem.Services.DesignPanel.Adorners.Add(adornerPanel);

                BuildMaps(operation);

                if (operation.Type != PlacementType.Resize && operation.PlacedItems.Count == 1)
                {
                    baseline = GetBaseline(operation.PlacedItems[0].Item.View);
                }
            }
        }

        private IEnumerable<DesignItem> AllDesignItems(DesignItem designItem = null)
        {
            if (designItem == null && this.ExtendedItem.Services.DesignPanel is DesignPanel)
                designItem = this.ExtendedItem.Services.DesignPanel.Context.RootItem;

            if (designItem?.ContentProperty != null)
            {
                if (designItem.ContentProperty.IsCollection)
                {
                    foreach (var collectionElement in designItem.ContentProperty.CollectionElements)
                    {
                        if (collectionElement != null)
                            yield return collectionElement;

                        foreach (var el in AllDesignItems(collectionElement))
                        {
                            if (el != null)
                                yield return el;
                        }
                    }
                }
                else if (designItem.ContentProperty.Value != null)
                {
                    yield return designItem.ContentProperty.Value;

                    foreach (var el in AllDesignItems(designItem.ContentProperty.Value))
                    {
                        if (el != null)
                            yield return el;
                    }
                }
            }
        }

        /// <summary>
        /// Method Returns the DesignItems for wich Snaplines are created
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        protected IEnumerable<DesignItem> GetSnapToDesignItems(PlacementOperation operation)
        {
            return AllDesignItems();
        }

        void BuildMaps(PlacementOperation operation)
        {
            horizontalMap = new List<Snapline>();
            verticalMap = new List<Snapline>();

            var containerRect = new Rect(0, 0, ModelTools.GetWidth(ExtendedItem.View), ModelTools.GetHeight(ExtendedItem.View));
            if (SnaplineMargin > 0)
            {
                AddLines(containerRect, -SnaplineMargin, false);
            }

            AddLines(containerRect, 0, false);

            AddContainerSnaplines(containerRect, horizontalMap, verticalMap);

            if (!CanPlace(operation.PlacedItems.Select(x => x.Item).ToList(), operation.Type, PlacementAlignment.Center))
                return;

            foreach (var item in GetSnapToDesignItems(operation)
                     .Except(operation.PlacedItems.Select(f => f.Item))
                     .Where(x => x.View != null && !GetDisableSnaplines(x.View)))
            {
                if (item != null)
                {
                    var bounds = GetPositionRelativeToContainer(operation, item);

                    AddLines(bounds, 0, false);
                    if (SnaplineMargin > 0)
                    {
                        AddLines(bounds, SnaplineMargin, true);
                    }
                    AddBaseline(item, bounds, horizontalMap);
                }
            }
        }

        protected virtual void AddContainerSnaplines(Rect containerRect, List<SnaplinePlacementBehavior.Snapline> horizontalMap, List<SnaplinePlacementBehavior.Snapline> verticalMap)
        {
        }

        void AddLines(Rect r, double inflate, bool requireOverlap)
        {
            AddLines(r, inflate, requireOverlap, horizontalMap, verticalMap, null);
        }

        void AddLines(Rect r, double inflate, bool requireOverlap, List<Snapline> h, List<Snapline> v, PlacementAlignment? filter)
        {
            if (!r.IsEmpty())
            {
                Rect r2 = r;
                r2 = r2.Inflate(inflate, inflate);

                if (filter == null || filter.Value.Vertical == VerticalAlignment.Top)
                    h.Add(new Snapline() { RequireOverlap = requireOverlap, Offset = r2.Top - 1, Start = r.Left, End = r.Right });
                if (filter == null || filter.Value.Vertical == VerticalAlignment.Bottom)
                    h.Add(new Snapline() { RequireOverlap = requireOverlap, Offset = r2.Bottom - 1, Start = r.Left, End = r.Right });
                if (filter == null || filter.Value.Horizontal == HorizontalAlignment.Left)
                    v.Add(new Snapline() { RequireOverlap = requireOverlap, Offset = r2.Left - 1, Start = r.Top, End = r.Bottom });
                if (filter == null || filter.Value.Horizontal == HorizontalAlignment.Right)
                    v.Add(new Snapline() { RequireOverlap = requireOverlap, Offset = r2.Right - 1, Start = r.Top, End = r.Bottom });

                if (filter == null)
                {
                    h.Add(new Snapline() { RequireOverlap = requireOverlap, Offset = r2.Top + Math.Abs((r2.Top - r2.Bottom) / 2) - 1, DrawOffset = 1, Start = r.Left, End = r.Right });
                    v.Add(new Snapline() { RequireOverlap = requireOverlap, Offset = r2.Left + Math.Abs((r2.Left - r2.Right) / 2) - 1, DrawOffset = 1, Start = r.Top, End = r.Bottom });
                }
            }
        }

        void AddBaseline(DesignItem item, Rect bounds, List<Snapline> list)
        {
            var baseline = GetBaseline(item.View);
            if (baseline.HasValue)
            {
                var textOffset = item.View.TranslatePoint(new Point(0, baseline.Value), ExtendedItem.View).Value.Y;
                list.Add(new Snapline() { Group = 1, Offset = textOffset, Start = bounds.Left, End = bounds.Right });
            }
        }

        void DeleteSurface()
        {
            if (surface != null)
            {
                ExtendedItem.Services.DesignPanel.Adorners.Remove(adornerPanel);
                adornerPanel = null;
                surface = null;
                horizontalMap = null;
                verticalMap = null;
            }
        }

        void DrawLine(double x1, double y1, double x2, double y2)
        {
            if (double.IsInfinity(x1) || double.IsNaN(x1) || double.IsInfinity(y1) || double.IsNaN(y1) ||
    double.IsInfinity(x2) || double.IsNaN(x2) || double.IsInfinity(y2) || double.IsNaN(y2))
                return;

            var line1 = new Line()
            {
                StartPoint = new Point(x1, y1),
                EndPoint = new Point(x2, y2),
                StrokeThickness = 1,
                Stroke = Brushes.White
            };
            surface.Children.Add(line1);

            var line2 = new Line()
            {
                StartPoint = new Point(x1, y1),
                EndPoint = new Point(x2, y2),
                StrokeThickness = 1,
                Stroke = Brushes.Orange,
                StrokeDashArray = new AvaloniaList<double>(new double[] { 5, 2 }),
                StrokeDashOffset = x1 + y1  // fix dashes
            };
            surface.Children.Add(line2);
        }

        //TODO: GlyphRun must be used
        static double? GetBaseline(Control element)
        {
            var textBox = element as TextBox;
            if (textBox != null)
            {
                var r = textBox.GetRectFromCharacterIndex(0).Bottom;
                return textBox.TranslatePoint(new Point(0, r), element).Value.Y;
            }
            var textBlock = element as TextBlock;
            if (textBlock != null)
                return textBlock.TranslatePoint(new Point(0, textBlock.Bounds.Height), element).Value.Y;

            return null;
        }

        static bool Snap(List<Snapline> input, List<Snapline> map, double accuracy,
                         out List<Snapline> drawLines, out double delta)
        {
            delta = double.MaxValue;
            drawLines = null;

            foreach (var inputLine in input)
            {
                foreach (var mapLine in map)
                {
                    if (Math.Abs(mapLine.Offset - inputLine.Offset) <= accuracy)
                    {
                        if (!inputLine.RequireOverlap && !mapLine.RequireOverlap ||
                            Math.Max(inputLine.Start, mapLine.Start) < Math.Min(inputLine.End, mapLine.End))
                        {
                            if (mapLine.Group == inputLine.Group)
                                delta = mapLine.Offset - inputLine.Offset;
                        }
                    }
                }
            }

            if (delta == double.MaxValue) return false;
            var offsetDict = new Dictionary<double, Snapline>();

            foreach (var inputLine in input)
            {
                inputLine.Offset += delta;
                foreach (var mapLine in map)
                {
                    if (inputLine.Offset == mapLine.Offset)
                    {
                        var offset = mapLine.Offset;
                        Snapline drawLine;
                        if (!offsetDict.TryGetValue(offset, out drawLine))
                        {
                            drawLine = new Snapline();
                            drawLine.Start = double.MaxValue;
                            drawLine.End = double.MinValue;
                            offsetDict[offset] = drawLine;
                        }
                        drawLine.Offset = offset;
                        drawLine.Start = Math.Min(drawLine.Start, Math.Min(inputLine.Start, mapLine.Start));
                        drawLine.End = Math.Max(drawLine.End, Math.Max(inputLine.End, mapLine.End));
                    }
                }
            }

            drawLines = offsetDict.Values.ToList();
            return true;
        }

        [DebuggerDisplay("Snapline: {Offset}")]
        public class Snapline
        {
            public double Offset;
            public double Start;
            public double End;
            public bool RequireOverlap;
            public int Group;

            public double DrawOffset = 0;
        }
    }
}
