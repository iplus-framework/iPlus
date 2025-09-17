// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Services;
using gip.ext.design.avui;
using Avalonia.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace gip.ext.designer.avui.Controls
{
    /// <summary>
    /// Gray out everything except a specific area.
    /// </summary>
    public sealed class GrayOutDesignerExceptActiveArea : Control
    {
        Geometry designSurfaceRectangle;
        Geometry activeAreaGeometry;
        Geometry combinedGeometry;
        Brush grayOutBrush;
        AdornerPanel adornerPanel;
        IDesignPanel designPanel;
        const double MaxOpacity = 0.3;

        public GrayOutDesignerExceptActiveArea()
        {
            this.GrayOutBrush = new SolidColorBrush(Colors.Gray);
            this.GrayOutBrush.Opacity = MaxOpacity;
            this.IsHitTestVisible = false;
        }

        public Brush GrayOutBrush
        {
            get { return grayOutBrush; }
            set { grayOutBrush = value; }
        }

        public Geometry ActiveAreaGeometry
        {
            get { return activeAreaGeometry; }
            set
            {
                activeAreaGeometry = value;
                combinedGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, designSurfaceRectangle, activeAreaGeometry);
            }
        }

        public override void Render(DrawingContext drawingContext)
        {
            base.Render(drawingContext);
            drawingContext.DrawGeometry(grayOutBrush, null, combinedGeometry);
        }

        Rect currentAnimateActiveAreaRectToTarget;

        internal void AnimateActiveAreaRectTo(Rect newRect)
        {
            if (newRect.Equals(currentAnimateActiveAreaRectToTarget))
                return;

            // For Avalonia, we'll do a simple direct update
            // Animation can be added later with proper Avalonia animation framework
            if (activeAreaGeometry is RectangleGeometry rectGeometry)
            {
                rectGeometry.Rect = newRect;
            }

            currentAnimateActiveAreaRectToTarget = newRect;
        }

        internal static void Start(ref GrayOutDesignerExceptActiveArea grayOut, ServiceContainer services, Control activeContainer)
        {
            Debug.Assert(activeContainer != null);
            Start(ref grayOut, services, activeContainer, new Rect(activeContainer.Bounds.Size));
        }

        internal static void Start(ref GrayOutDesignerExceptActiveArea grayOut, ServiceContainer services, Control activeContainer, Rect activeRectInActiveContainer)
        {
            Debug.Assert(services != null);
            Debug.Assert(activeContainer != null);
            DesignPanel designPanel = services.GetService<IDesignPanel>() as DesignPanel;
            OptionService optionService = services.GetService<OptionService>();
            if (designPanel != null && grayOut == null && optionService != null && optionService.GrayOutDesignSurfaceExceptParentContainerWhenDragging)
            {
                grayOut = new GrayOutDesignerExceptActiveArea();
                grayOut.designSurfaceRectangle = new RectangleGeometry(
                    new Rect(new Point(0, 0), designPanel.Bounds.Size));
                grayOut.designPanel = designPanel;
                grayOut.adornerPanel = new AdornerPanel();
                grayOut.adornerPanel.Order = AdornerOrder.BehindForeground;
                grayOut.adornerPanel.SetAdornedElement(designPanel.Context.RootItem.View, null);
                grayOut.adornerPanel.Children.Add(grayOut);

                // Create transform matrix for active container
                var transform = activeContainer.TransformToVisual(grayOut.adornerPanel.AdornedElement as Visual);
                var transformMatrix = transform ?? Matrix.Identity;

                grayOut.ActiveAreaGeometry = new RectangleGeometry(activeRectInActiveContainer)
                {
                    Transform = new MatrixTransform(transformMatrix)
                };

                AnimateOpacity(grayOut, MaxOpacity);
                designPanel.Adorners.Add(grayOut.adornerPanel);
            }
        }

        static readonly TimeSpan animationTime = TimeSpan.FromMilliseconds(200);

        static void AnimateOpacity(GrayOutDesignerExceptActiveArea grayOut, double to)
        {
            // Use Avalonia's built-in opacity animation on the control itself
            if (grayOut.GrayOutBrush is SolidColorBrush brush)
            {
                // Create a timer-based fade effect
                var startOpacity = brush.Opacity;
                var steps = 10;
                var stepTime = animationTime.TotalMilliseconds / steps;
                var opacityStep = (to - startOpacity) / steps;
                var currentStep = 0;

                var timer = DispatcherTimer.Run(() =>
                {
                    currentStep++;
                    brush.Opacity = startOpacity + (opacityStep * currentStep);

                    if (currentStep >= steps)
                    {
                        brush.Opacity = to;
                        return false; // Stop timer
                    }
                    return true; // Continue timer
                }, TimeSpan.FromMilliseconds(stepTime));
            }
        }

        internal static void Stop(ref GrayOutDesignerExceptActiveArea grayOut)
        {
            if (grayOut != null)
            {
                AnimateOpacity(grayOut, 0);
                IDesignPanel designPanel = grayOut.designPanel;
                AdornerPanel adornerPanelToRemove = grayOut.adornerPanel;

                var timer = DispatcherTimer.Run(() =>
                {
                    designPanel.Adorners.Remove(adornerPanelToRemove);
                    return false; // Don't repeat
                }, animationTime);

                grayOut = null;
            }
        }
    }
}
