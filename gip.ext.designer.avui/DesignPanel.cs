// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui;
using gip.ext.designer.avui.Xaml;
using System.Linq;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Input;
using gip.ext.design.avui.Extensions;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace gip.ext.designer.avui
{
    public sealed class DesignPanel : Decorator, IDesignPanel, INotifyPropertyChanged
    {
        #region Hit Testing

        private List<DesignItem> hitTestElements = new List<DesignItem>();
        private List<DesignItem> skippedHitTestElements = new List<DesignItem>();

        /// <summary>
        /// this element is always hit (unless HitTestVisible is set to false)
        /// </summary>
        sealed class EatAllHitTestRequests : Control
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

        void RunHitTest(Visual reference, Point point, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback)
        {
            if (!Keyboard.IsKeyDown(Key.LeftAlt))
            {
                hitTestElements.Clear();
                skippedHitTestElements.Clear();
            }

            VisualTreeHelper.HitTest(reference, filterCallback, resultCallback, new PointHitTestParameters(point));
        }

        HitTestFilterBehavior FilterHitTestInvisibleElements(AvaloniaObject potentialHitTestTarget, HitTestType hitTestType)
        {
            Control element = potentialHitTestTarget as Control;

            if (element != null)
            {
                if (!(element.IsHitTestVisible && element.IsVisible))
                {
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                }

                var designItem = Context.Services.Component.GetDesignItem(element) as XamlDesignItem;

                if (hitTestType == HitTestType.ElementSelection)
                {
                    if (Keyboard.IsKeyDown(Key.LeftAlt))
                    {
                        if (designItem != null)
                        {
                            if (skippedHitTestElements.LastOrDefault() == designItem ||
                                (hitTestElements.Contains(designItem) && !skippedHitTestElements.Contains(designItem)))
                            {
                                skippedHitTestElements.Remove(designItem);
                                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                            }
                        }
                    }
                }
                else
                {
                    hitTestElements.Clear();
                    skippedHitTestElements.Clear();
                }

                if (designItem != null && designItem.IsDesignTimeLocked)
                {
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                }

                if (designItem != null && !hitTestElements.Contains(designItem))
                {
                    hitTestElements.Add(designItem);
                    skippedHitTestElements.Add(designItem);
                }
            }

            return HitTestFilterBehavior.Continue;
        }

        /// <summary>
        /// Performs a custom hit testing lookup for the specified mouse event args.
        /// </summary>
        public DesignPanelHitTestResult HitTest(Point mousePosition, bool testAdorners, bool testDesignSurface, HitTestType hitTestType = HitTestType.Default)
        {
            DesignPanelHitTestResult result = DesignPanelHitTestResult.NoHit;
            HitTest(mousePosition, testAdorners, testDesignSurface,
                    delegate(DesignPanelHitTestResult r)
                    {
                        result = r;
                        return false;
                    },
                    hitTestType);
            return result;
        }

        /// <summary>
        /// Performs a hit test on the design surface, raising <paramref name="callback"/> for each match.
        /// Hit testing continues while the callback returns true.
        /// </summary>
        public void HitTest(Point mousePosition, bool testAdorners, bool testDesignSurface, Predicate<DesignPanelHitTestResult> callback, HitTestType hitTestType = HitTestType.Default)
        {
            if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X > this.Bounds.Width || mousePosition.Y > this.Bounds.Height)
            {
                return;
            }
            // First try hit-testing on the adorner layer.

            bool continueHitTest = true;

            HitTestFilterCallback filterBehavior = CustomHitTestFilterBehavior ?? (x => FilterHitTestInvisibleElements(x, hitTestType));
            CustomHitTestFilterBehavior = null;

            if (testAdorners)
            {
                RunHitTest(
                    _adornerLayer, mousePosition, filterBehavior,
                    delegate(HitTestResult result)
                    {
                        if (result != null && result.VisualHit != null && result.VisualHit is Visual)
                        {
                            DesignPanelHitTestResult customResult = new DesignPanelHitTestResult((Visual)result.VisualHit);
                            AvaloniaObject obj = result.VisualHit;
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
                            AvaloniaObject objVisualHit = result.VisualHit;
                            AvaloniaObject obj = result.VisualHit;
                            
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
            //DesignerProperties.SetIsInDesignMode(this, true);

            _eatAllHitTestRequests = new EatAllHitTestRequests();
            _eatAllHitTestRequests.MouseDown += delegate
            {
                // ensure the design panel has focus while the user is interacting with it
                this.Focus();
            };
            _eatAllHitTestRequests.AllowDrop = true;
            _adornerLayer = new AdornerLayer(this);

            this.KeyUp += DesignPanel_KeyUp;
            this.KeyDown += DesignPanel_KeyDown;
        }
        #endregion

        #region Properties

        public DesignSurface DesignSurface { get; internal set; }

        //Set custom HitTestFilterCallbak
        public HitTestFilterCallback CustomHitTestFilterBehavior { get; set; }

        public AdornerLayer AdornerLayer
        {
            get
            {
                return _adornerLayer;
            }
        }

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

        /// <summary>
        /// Enables / Disables the Snapline Placement
        /// </summary>
        private bool _useSnaplinePlacement = true;
        public bool UseSnaplinePlacement
        {
            get { return _useSnaplinePlacement; }
            set
            {
                if (_useSnaplinePlacement != value)
                {
                    _useSnaplinePlacement = value;
                    OnPropertyChanged("UseSnaplinePlacement");
                }
            }
        }

        /// <summary>
        /// Enables / Disables the Raster Placement
        /// </summary>
        private bool _useRasterPlacement = false;
        public bool UseRasterPlacement
        {
            get { return _useRasterPlacement; }
            set
            {
                if (_useRasterPlacement != value)
                {
                    _useRasterPlacement = value;
                    OnPropertyChanged("UseRasterPlacement");
                }
            }
        }

        /// <summary>
        /// Sets the with of the Raster when using Raster Placement
        /// </summary>
        private int _rasterWidth = 5;
        public int RasterWidth
        {
            get { return _rasterWidth; }
            set
            {
                if (_rasterWidth != value)
                {
                    _rasterWidth = value;
                    OnPropertyChanged("RasterWidth");
                }
            }
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
        public override Control Child
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

        PlacementOperation placementOp;
        Dictionary<PlacementInformation, int> dx = new Dictionary<PlacementInformation, int>();
        Dictionary<PlacementInformation, int> dy = new Dictionary<PlacementInformation, int>();

        /// <summary>
        /// If interface implementing class sets this to false defaultkeyaction will be 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool InvokeDefaultKeyDownAction(Extension e)
        {
            var keyDown = e as IKeyDown;
            if (keyDown != null)
            {
                return keyDown.InvokeDefaultAction;
            }

            return true;
        }

        private void DesignPanel_KeyUp(object sender, KeyEventArgs e)
        {
            // Only previe events
            if (e.Route != Avalonia.Interactivity.RoutingStrategies.Tunnel)
                return;
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                e.Handled = true;

                if (placementOp != null)
                {
                    placementOp.Commit();
                    placementOp = null;
                    dx.Clear();
                    dy.Clear();
                }
            }
            //pass the key event to the underlying objects if they have implemented IKeyUp interface
            //OBS!!!! this call needs to be here, after the placementOp.Commit().
            //In case the underlying object has a operation of its own this operation needs to be commited first
            foreach (DesignItem di in Context.Services.Selection.SelectedItems.Reverse())
            {
                foreach (Extension ext in di.Extensions)
                {
                    var keyUp = ext as IKeyUp;
                    if (keyUp != null)
                    {
                        keyUp.KeyUpAction(sender, e);
                    }
                }
            }
        }

        void DesignPanel_KeyDown(object sender, KeyEventArgs e)
        {
            // Only previe events
            if (e.Route != Avalonia.Interactivity.RoutingStrategies.Tunnel)
                return;
            //pass the key event down to the underlying objects if they have implemented IKeyUp interface
            //OBS!!!! this call needs to be here, before the PlacementOperation.Start.
            //In case the underlying object has a operation of its own this operation needs to be set first
            foreach (DesignItem di in Context.Services.Selection.SelectedItems)
            {
                foreach (Extension ext in di.Extensions)
                {
                    var keyDown = ext as IKeyDown;
                    if (keyDown != null)
                    {
                        keyDown.KeyDownAction(sender, e);
                    }
                }
            }

            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                bool initialEvent = false;

                e.Handled = true;

                PlacementType placementType = Keyboard.IsKeyDown(Key.LeftCtrl) ? PlacementType.Resize : PlacementType.MoveAndIgnoreOtherContainers;

                if (placementOp != null && placementOp.Type != placementType)
                {
                    placementOp.Commit();
                    placementOp = null;
                    dx.Clear();
                    dy.Clear();
                }

                if (placementOp == null)
                {

                    //check if any objects don't want the default action to be invoked
                    List<DesignItem> placedItems = Context.Services.Selection.SelectedItems.Where(x => x.Extensions.All(InvokeDefaultKeyDownAction)).ToList();

                    //if no remaining objects, break
                    if (placedItems.Count < 1) return;

                    placementOp = PlacementOperation.Start(placedItems, placementType);

                    dx.Clear();
                    dy.Clear();
                }

                int odx = 0, ody = 0;
                switch (e.Key)
                {
                    case Key.Left:
                        odx = Keyboard.IsKeyDown(Key.LeftShift) ? -10 : -1;
                        break;
                    case Key.Up:
                        ody = Keyboard.IsKeyDown(Key.LeftShift) ? -10 : -1;
                        break;
                    case Key.Right:
                        odx = Keyboard.IsKeyDown(Key.LeftShift) ? 10 : 1;
                        break;
                    case Key.Down:
                        ody = Keyboard.IsKeyDown(Key.LeftShift) ? 10 : 1;
                        break;
                }

                foreach (PlacementInformation info in placementOp.PlacedItems)
                {
                    if (!dx.ContainsKey(info))
                    {
                        dx[info] = 0;
                        dy[info] = 0;
                    }
                    var transform = info.Item.Parent.View.TransformToVisual(this);
                    var mt = transform as MatrixTransform;
                    if (mt != null)
                    {
                        var angle = Math.Atan2(mt.Matrix.M21, mt.Matrix.M11) * 180 / Math.PI;
                        if (angle > 45.0 && angle < 135.0)
                        {
                            dx[info] += ody * -1;
                            dy[info] += odx;
                        }
                        else if (angle < -45.0 && angle > -135.0)
                        {
                            dx[info] += ody;
                            dy[info] += odx * -1;
                        }
                        else if (angle > 135.0 || angle < -135.0)
                        {
                            dx[info] += odx * -1;
                            dy[info] += ody * -1;
                        }
                        else
                        {
                            dx[info] += odx;
                            dy[info] += ody;
                        }
                    }

                    var bounds = info.OriginalBounds;

                    if (placementType == PlacementType.Move
                        || info.Operation.Type == PlacementType.MoveAndIgnoreOtherContainers)
                    {
                        info.Bounds = new Rect(bounds.Left + dx[info],
                                               bounds.Top + dy[info],
                                               bounds.Width,
                                               bounds.Height);
                    }
                    else if (placementType == PlacementType.Resize)
                    {
                        if (bounds.Width + dx[info] >= 0 && bounds.Height + dy[info] >= 0)
                        {
                            info.Bounds = new Rect(bounds.Left,
                                                   bounds.Top,
                                                   bounds.Width + dx[info],
                                                   bounds.Height + dy[info]);
                        }
                    }

                    placementOp.CurrentContainerBehavior.SetPosition(info);
                }
            }
        }

        static bool IsPropertySet(Control element, AvaloniaProperty d)
        {
            return element.ReadLocalValue(d) != AvaloniaProperty.UnsetValue;
        }

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

        //public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DragEventArgs> DragEnter;
        public event EventHandler<DragEventArgs> DragOver;
        public event EventHandler<DragEventArgs> DragLeave;
        public event EventHandler<DragEventArgs> Drop;

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
        }

        //protected override void OnGotFocus(RoutedEventArgs e)
        //{
        //    //Comment because causes scrollviewer jump when is Focus method called!
        //    //base.OnGotFocus(e);
        //}

        #region ContextMenu

        private Dictionary<ContextMenu, Tuple<int, List<object>>> contextMenusAndEntries = new Dictionary<ContextMenu, Tuple<int, List<object>>>();

        public Action<ContextMenu> ContextMenuHandler { get; set; }

        public void AddContextMenu(ContextMenu contextMenu)
        {
            AddContextMenu(contextMenu, int.MaxValue);
        }

        public void AddContextMenu(ContextMenu contextMenu, int orderIndex)
        {
            contextMenusAndEntries.Add(contextMenu, new Tuple<int, List<object>>(orderIndex, new List<object>(contextMenu.Items.Cast<object>())));
            contextMenu.Items.Clear();

            UpdateContextMenu();
        }

        public void RemoveContextMenu(ContextMenu contextMenu)
        {
            contextMenusAndEntries.Remove(contextMenu);

            UpdateContextMenu();
        }

        public void ClearContextMenu()
        {
            contextMenusAndEntries.Clear();
            ContextMenu = null;
        }

        private void UpdateContextMenu()
        {
            if (this.ContextMenu != null)
            {
                this.ContextMenu.Items.Clear();
                this.ContextMenu = null;
            }

            var contextMenu = new ContextMenu();

            foreach (var entries in contextMenusAndEntries.Values.OrderBy(x => x.Item1).Select(x => x.Item2))
            {
                if (contextMenu.Items.Count > 0)
                    contextMenu.Items.Add(new Separator());

                foreach (var entry in entries)
                {
                    var ctl = ((FrameworkElement)entry).TryFindParent<ItemsControl>();
                    if (ctl != null)
                        ctl.Items.Remove(entry);
                    contextMenu.Items.Add(entry);
                }
            }

            if (ContextMenuHandler != null)
                ContextMenuHandler(contextMenu);
            else
                this.ContextMenu = contextMenu;
        }

        public void HitTest(Point mousePosition, bool testAdorners, bool testDesignSurface, Predicate<DesignPanelHitTestResult> callback)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
