// Copyright (c) 2019 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using gip.ext.design.avui;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.design.avui.UIExtensions;
using gip.ext.designer.avui.Controls;

namespace gip.ext.designer.avui.Extensions
{
    /// <summary>
    /// The resize thumb around a component.
    /// </summary>
    [ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
    [ExtensionFor(typeof(Control))]
    public sealed class RotateThumbExtension : SelectionAdornerProvider
    {
        readonly AdornerPanel adornerPanel;
        readonly Thumb thumb;
        /// <summary>An array containing this.ExtendedItem as only element</summary>
        readonly DesignItem[] extendedItemArray = new DesignItem[1];
        IPlacementBehavior resizeBehavior;
        PlacementOperation operation;

        public RotateThumbExtension()
        {
            adornerPanel = new AdornerPanel();
            adornerPanel.Order = AdornerOrder.Foreground;
            this.Adorners.Add(adornerPanel);

            thumb = CreateRotateThumb();
        }

        DesignerThumb CreateRotateThumb()
        {
            DesignerThumb rotateThumb = new RotateThumb();
            rotateThumb.Cursor = new Cursor(StandardCursorType.Hand);
            rotateThumb.Cursor = ZoomControl.GetCursor("Images/rotate.cur");
            rotateThumb.Alignment = PlacementAlignment.Top;
            AdornerPanel.SetPlacement(rotateThumb,
                                      new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Top) { WidthRelativeToContentWidth = 1, HeightOffset = 0 });
            adornerPanel.Children.Add(rotateThumb);

            DragListener drag = new DragListener(rotateThumb);
            drag.Started += drag_Rotate_Started;
            drag.Changed += drag_Rotate_Changed;
            drag.Completed += drag_Rotate_Completed;
            return rotateThumb;
        }

        #region Rotate

        private Point centerPoint;
        private Control parent;
        private Vector startVector;
        private RotateTransform rotateTransform;
        private double initialAngle;
        private DesignItem rtTransform;

        private void drag_Rotate_Started(DragListener drag)
        {
            if (drag == null || drag.LastEventArgs == null)
                return;

            var designerItem = this.ExtendedItem.Component as Control;
            this.parent = VisualTreeHelper.GetParent(designerItem) as Control;
            this.centerPoint = designerItem.TranslatePoint(
                new Point(designerItem.Bounds.Width * designerItem.RenderTransformOrigin.Point.X,
                          designerItem.Bounds.Height * designerItem.RenderTransformOrigin.Point.Y),
                this.parent).Value;

            Point startPoint = drag.LastEventArgs.GetPosition(this.parent);
            this.startVector = startPoint - this.centerPoint;

            if (this.rotateTransform == null)
            {
                this.initialAngle = 0;
            }
            else
            {
                this.initialAngle = this.rotateTransform.Angle;
            }

            rtTransform = this.ExtendedItem.Properties[Control.RenderTransformProperty].Value;

            operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
        }

        private void drag_Rotate_Changed(DragListener drag)
        {
            if (drag == null || drag.LastEventArgs == null)
                return;

            Point currentPoint = drag.LastEventArgs.GetPosition(this.parent);
            Vector deltaVector = currentPoint - this.centerPoint;

            double angle = VectorExtensions.AngleBetween(this.startVector, deltaVector);

            var destAngle = this.initialAngle + Math.Round(angle, 0);

            if (!IsKeyDown(Key.LeftCtrl))
                destAngle = ((int)destAngle / 15) * 15;

            ModelTools.ApplyTransform(this.ExtendedItem, new RotateTransform() { Angle = destAngle }, false);
        }

        void drag_Rotate_Completed(DragListener drag)
        {
            operation.Commit();
        }

        #endregion

        protected override void OnInitialized()
        {
            if (this.ExtendedItem.Component is WindowClone)
                return;
            base.OnInitialized();
            extendedItemArray[0] = this.ExtendedItem;
            this.Services.Selection.PrimarySelectionChanged += OnPrimarySelectionChanged;
            resizeBehavior = PlacementOperation.GetPlacementBehavior(extendedItemArray);
            OnPrimarySelectionChanged(null, null);

            var designerItem = this.ExtendedItem.Component as Control;
            this.rotateTransform = designerItem.RenderTransform as RotateTransform;
            if (this.rotateTransform == null)
            {
                var tg = designerItem.RenderTransform as TransformGroup;
                if (tg != null)
                {
                    this.rotateTransform = tg.Children.FirstOrDefault(x => x is RotateTransform) as RotateTransform;
                }
            }

        }

        protected override void OnRemove()
        {
            this.Services.Selection.PrimarySelectionChanged -= OnPrimarySelectionChanged;
            base.OnRemove();
        }

        void OnPrimarySelectionChanged(object sender, EventArgs e)
        {
            bool isPrimarySelection = this.Services.Selection.PrimarySelection == this.ExtendedItem;
            foreach (RotateThumb g in adornerPanel.Children)
            {
                g.IsPrimarySelection = isPrimarySelection;
            }
        }
    }
}
