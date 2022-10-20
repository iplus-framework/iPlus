// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Shapes;

using gip.ext.design.Adorners;
using gip.ext.designer.Controls;
using gip.ext.design;

namespace gip.ext.designer
{
    public sealed class DesignPanel : Decorator, IDesignPanel
    {
        #region Hit Testing
        /// <summary>
        /// this element is always hit (unless HitTestVisible is set to false)
        /// </summary>
        sealed class EatAllHitTestRequests : UIElement
        {
            protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
            {
                return new GeometryHitTestResult(this, IntersectionDetail.FullyContains);
            }

            protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
            {
                return new PointHitTestResult(this, hitTestParameters.HitPoint);
            }
        }

        static void RunHitTest(Visual reference, Point point, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback)
        {
            VisualTreeHelper.HitTest(reference, filterCallback, resultCallback,
                                     new PointHitTestParameters(point));
        }

        static HitTestFilterBehavior FilterHitTestInvisibleElements(DependencyObject potentialHitTestTarget)
        {
            UIElement element = potentialHitTestTarget as UIElement;
            if (element != null)
            {
                if (!(element.IsHitTestVisible && element.Visibility == Visibility.Visible))
                {
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                }
            }
            return HitTestFilterBehavior.Continue;
        }

        /// <summary>
        /// Performs a custom hit testing lookup for the specified mouse event args.
        /// </summary>
        public DesignPanelHitTestResult HitTest(Point mousePosition, bool testAdorners, bool testDesignSurface)
        {
            DesignPanelHitTestResult result = DesignPanelHitTestResult.NoHit;
            HitTest(mousePosition, testAdorners, testDesignSurface,
                    delegate(DesignPanelHitTestResult r)
                    {
                        result = r;
                        return false;
                    });
            return result;
        }

        /// <summary>
        /// Performs a hit test on the design surface, raising <paramref name="callback"/> for each match.
        /// Hit testing continues while the callback returns true.
        /// </summary>
        public void HitTest(Point mousePosition, bool testAdorners, bool testDesignSurface, Predicate<DesignPanelHitTestResult> callback)
        {
            if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X > this.RenderSize.Width || mousePosition.Y > this.RenderSize.Height)
            {
                return;
            }
            // First try hit-testing on the adorner layer.

            bool continueHitTest = true;

            if (testAdorners)
            {
                RunHitTest(
                    _adornerLayer, mousePosition, FilterHitTestInvisibleElements,
                    delegate(HitTestResult result)
                    {
                        if (result != null && result.VisualHit != null && result.VisualHit is Visual)
                        {
                            DesignPanelHitTestResult customResult = new DesignPanelHitTestResult((Visual)result.VisualHit);
                            DependencyObject obj = result.VisualHit;
                            while (obj != null && obj != _adornerLayer)
                            {
                                AdornerPanel adorner = obj as AdornerPanel;
                                if (adorner != null)
                                {
                                    customResult.AdornerHit = adorner;
                                }
                                obj = VisualTreeHelper.GetParent(obj);
                            }
                            continueHitTest = callback(customResult);
                            return continueHitTest ? HitTestResultBehavior.Continue : HitTestResultBehavior.Stop;
                        }
                        else
                        {
                            return HitTestResultBehavior.Continue;
                        }
                    });
            }

            if (continueHitTest && testDesignSurface)
            {
                RunHitTest(
                    this.Child, mousePosition, delegate { return HitTestFilterBehavior.Continue; },
                    delegate(HitTestResult result)
                    {
                        if (result != null && result.VisualHit != null && result.VisualHit is Visual)
                        {
                            DesignPanelHitTestResult customResult = new DesignPanelHitTestResult((Visual)result.VisualHit);

                            if (_context == null)
                                return HitTestResultBehavior.Stop;

                            ViewService viewService = _context.Services.View;
                            DependencyObject objVisualHit = result.VisualHit;
                            DependencyObject obj = result.VisualHit;
                            
                            // Search Element in the HitTestLayer 
                            if (HitTestLayer >= 0)
                            {
                                List<DesignItem> layerList = new List<DesignItem>();
                                List<DesignItem> layerListViewBox = new List<DesignItem>();
                                while (obj != null)
                                {
                                    DesignItem itemInXAMLTree = viewService.GetModel(obj);
                                    if (itemInXAMLTree != null)
                                    {
                                        layerList.Add(itemInXAMLTree);
                                        if (HitTestGroupedPreferred && (obj is Viewbox))
                                            layerListViewBox.Add(itemInXAMLTree);
                                    }
                                    obj = VisualTreeHelper.GetParent(obj);
                                }

                                if (HitTestGroupedPreferred)
                                {
                                    if (layerListViewBox.Count > 0)
                                    {
                                        // Select Viewbox from Layer
                                        if (layerListViewBox.Count >= (HitTestLayer + 1))
                                            customResult.ModelHit = layerListViewBox[layerListViewBox.Count - HitTestLayer - 1];
                                        // Select first element (deepest) from List
                                        else
                                        {
                                            foreach (DesignItem itemInXAMLTree in layerListViewBox)
                                            {
                                                if ((customResult.ModelHit = itemInXAMLTree) != null)
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (layerList.Count > 0)
                                        {
                                            customResult.ModelHit = layerList[layerList.Count - 1];
                                        }
                                    }
                                }
                                else
                                {
                                    if (layerList.Count > 0)
                                    {
                                        // If Hierarchy larger than HitTestLayer, then pick the element at this Layer
                                        if (layerList.Count >= (HitTestLayer+1))
                                            customResult.ModelHit = layerList[layerList.Count - HitTestLayer - 1];
                                        // Select first element (deepest) from List
                                        else
                                        {
                                            foreach (DesignItem itemInXAMLTree in layerList)
                                            {
                                                if ((customResult.ModelHit = itemInXAMLTree) != null)
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                            // Search Deepest Element
                            else
                            {
                                DesignItem visualGroup = null;
                                DesignItem deepestItem = null;
                                while (obj != null)
                                {
                                    DesignItem itemInXAMLTree = viewService.GetModel(obj);
                                    if (itemInXAMLTree != null)
                                    {
                                        if (deepestItem == null)
                                            deepestItem = itemInXAMLTree;
                                        if (HitTestGroupedPreferred)
                                        {
                                            if (obj is Viewbox)
                                            {
                                                visualGroup = itemInXAMLTree;
                                                break;
                                            }
                                        }
                                        else
                                            break;
                                    }
                                    obj = VisualTreeHelper.GetParent(obj);
                                }

                                if (HitTestGroupedPreferred)
                                {
                                    if (visualGroup != null)
                                        customResult.ModelHit = visualGroup;
                                    else
                                        customResult.ModelHit = _context.RootItem;
                                }
                                else
                                    customResult.ModelHit = deepestItem;
                            }

                            if (customResult.ModelHit == null)
                            {
                                customResult.ModelHit = _context.RootItem;
                            }
                            continueHitTest = callback(customResult);
                            return continueHitTest ? HitTestResultBehavior.Continue : HitTestResultBehavior.Stop;
                        }
                        else
                        {
                            return HitTestResultBehavior.Continue;
                        }
                    }
                );
            }
        }

        private Int32 _HitTestLayer = -1;
        public Int32 HitTestLayer
        {
            get
            {
                return _HitTestLayer;
            }

            set
            {
                _HitTestLayer = value;
            }
        }

        public bool HitTestGroupedPreferred
        {
            get;
            set;
        }
        #endregion

        #region Fields + Constructor
        DesignContext _context;
        readonly EatAllHitTestRequests _eatAllHitTestRequests;
        readonly AdornerLayer _adornerLayer;

        public DesignPanel()
        {
            this.Focusable = true;
            this.Margin = new Thickness(16);
            DesignerProperties.SetIsInDesignMode(this, true);

            _eatAllHitTestRequests = new EatAllHitTestRequests();
            _eatAllHitTestRequests.MouseDown += delegate
            {
                // ensure the design panel has focus while the user is interacting with it
                this.Focus();
            };
            _eatAllHitTestRequests.AllowDrop = true;
            _adornerLayer = new AdornerLayer(this);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets the design context.
        /// </summary>
        public DesignContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public ICollection<AdornerPanel> Adorners
        {
            get
            {
                return _adornerLayer.Adorners;
            }
        }

        /// <summary>
        /// Gets/Sets if the design content is visible for hit-testing purposes.
        /// </summary>
        public bool IsContentHitTestVisible
        {
            get { return !_eatAllHitTestRequests.IsHitTestVisible; }
            set { _eatAllHitTestRequests.IsHitTestVisible = !value; }
        }

        /// <summary>
        /// Gets/Sets if the adorner layer is visible for hit-testing purposes.
        /// </summary>
        public bool IsAdornerLayerHitTestVisible
        {
            get { return _adornerLayer.IsHitTestVisible; }
            set { _adornerLayer.IsHitTestVisible = value; }
        }

        private Double _RasterSize = 0;
        public Double RasterSize
        {
            get
            {
                return _RasterSize;
            }
            set
            {
                _RasterSize = value;
            }
        }

        public bool IsRasterOn
        {
            get
            {
                return AdornerRaster != null;
            }
        }

        internal DesignSurfaceRasterAdorner AdornerRaster
        {
            get;
            set;
        }

        #endregion

        #region Visual Child Management
        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (base.Child == value)
                    return;
                if (value == null)
                {
                    // Child is being set from some value to null

                    // remove _adornerLayer and _eatAllHitTestRequests
                    RemoveVisualChild(_adornerLayer);
                    RemoveVisualChild(_eatAllHitTestRequests);
                }
                else if (base.Child == null)
                {
                    // Child is being set from null to some value
                    AddVisualChild(_adornerLayer);
                    AddVisualChild(_eatAllHitTestRequests);
                }
                base.Child = value;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (base.Child != null)
            {
                if (index == 0)
                    return base.Child;
                else if (index == 1)
                    return _eatAllHitTestRequests;
                else if (index == 2)
                    return _adornerLayer;
            }
            return base.GetVisualChild(index);
        }

        protected override int VisualChildrenCount
        {
            get
            {
                if (base.Child != null)
                    return 3;
                else
                    return base.VisualChildrenCount;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size result = base.MeasureOverride(constraint);
            if (this.Child != null)
            {
                _adornerLayer.Measure(constraint);
                _eatAllHitTestRequests.Measure(constraint);
            }
            return result;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size result = base.ArrangeOverride(arrangeSize);
            if (this.Child != null)
            {
                Rect r = new Rect(new Point(0, 0), arrangeSize);
                _adornerLayer.Arrange(r);
                _eatAllHitTestRequests.Arrange(r);
            }
            return result;
        }
        #endregion

        protected override void OnQueryCursor(QueryCursorEventArgs e)
        {
            DesignPanelHitTestResult result = this.HitTest(e.GetPosition(this), false, true);
            // TODO: Prüfen ob immer Funktioniert
            if (result.VisualHit != null && result.VisualHit.GetType().Name != "VBCanvas")
            {
                //e.Cursor = Cursors.Hand;
                //e.Handled = true;
                //return;
            }

            base.OnQueryCursor(e);
            if (_context != null)
            {
                Cursor cursor = _context.Services.Tool.CurrentTool.Cursor;
                if (cursor != null)
                {
                    e.Cursor = cursor;
                    e.Handled = true;
                }
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            //Comment because causes scrollviewer jump when is Focus method called!
            //base.OnGotFocus(e);
        }
    }
}
